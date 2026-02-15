using System.Diagnostics;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HotTubShop.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using HotTubShop.Web.Models;
using HotTubShop.Web.Services;
using HotTubShop.Web.ViewModels;
using System.Text.Json;

namespace HotTubShop.Web.Controllers;

public class HomeController : Controller
{
    private const string CartSessionKey = "hottub_cart";
    private readonly ILogger<HomeController> _logger;
    private readonly IProductCatalogService _catalogService;
    private readonly IOrderMailService _orderMailService;
    private readonly AppDataPathProvider _appDataPathProvider;

    public HomeController(
        ILogger<HomeController> logger,
        IProductCatalogService catalogService,
        IOrderMailService orderMailService,
        AppDataPathProvider appDataPathProvider)
    {
        _logger = logger;
        _catalogService = catalogService;
        _orderMailService = orderMailService;
        _appDataPathProvider = appDataPathProvider;
    }

    public async Task<IActionResult> Index(string? lang)
    {
        var language = LanguageExtensions.NormalizeLanguage(lang);
        var products = await _catalogService.GetProductsAsync();

        return View(new HomeIndexViewModel
        {
            Language = language,
            Products = products
        });
    }

    [HttpGet]
    public async Task<IActionResult> Product(string id, string? lang)
    {
        var product = await _catalogService.GetByIdAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        return View(new ProductConfiguratorViewModel
        {
            Language = LanguageExtensions.NormalizeLanguage(lang),
            Product = product,
            SelectedByGroup = product.Options
                .GroupBy(o => o.GroupName)
                .ToDictionary(g => g.Key, _ => string.Empty)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Product(ProductConfiguratorViewModel model)
    {
        var product = await _catalogService.GetByIdAsync(model.Product.Id);
        if (product is null)
        {
            return NotFound();
        }

        var selectedOptionIds = (model.SelectedByGroup ?? new Dictionary<string, string>())
            .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
            .Select(kvp => kvp.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var selectedOptions = product.Options.Where(o => selectedOptionIds.Contains(o.Id)).ToList();

        model.Language = LanguageExtensions.NormalizeLanguage(model.Language);
        model.Product = product;
        model.SelectedByGroup = product.Options
            .GroupBy(o => o.GroupName)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var selected = g.FirstOrDefault(o => selectedOptionIds.Contains(o.Id));
                    return selected?.Id ?? string.Empty;
                });
        model.Configured = new ConfiguredHotTub
        {
            Product = product,
            SelectedOptions = selectedOptions
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(ProductConfiguratorViewModel model)
    {
        var product = await _catalogService.GetByIdAsync(model.Product.Id);
        if (product is null)
        {
            return NotFound();
        }

        var selectedOptionIds = (model.SelectedByGroup ?? new Dictionary<string, string>())
            .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
            .Select(kvp => kvp.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var selectedOptions = product.Options.Where(o => selectedOptionIds.Contains(o.Id)).ToList();

        var cart = GetCart();
        cart.Add(new CartItem
        {
            ProductId = product.Id,
            ProductName = product.LocalizedName(LanguageExtensions.NormalizeLanguage(model.Language)),
            ProductDescription = product.LocalizedDescription(LanguageExtensions.NormalizeLanguage(model.Language)),
            ProductImageUrl = product.ImageUrl,
            BasePrice = product.BasePrice,
            SelectedOptions = selectedOptions
        });
        SaveCart(cart);
        if (!(User.Identity?.IsAuthenticated ?? false))
        {
            TempData["ShowRegisterPrompt"] = "1";
        }

        return RedirectToAction(nameof(Cart), new { lang = model.Language });
    }

    [HttpGet]
    public IActionResult Cart(string? lang)
    {
        var language = LanguageExtensions.NormalizeLanguage(lang);
        return View(new CartViewModel
        {
            Language = language,
            Items = GetCart()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemoveFromCart(int index, string? lang)
    {
        var cart = GetCart();
        if (index >= 0 && index < cart.Count)
        {
            cart.RemoveAt(index);
            SaveCart(cart);
        }

        return RedirectToAction(nameof(Cart), new { lang = LanguageExtensions.NormalizeLanguage(lang) });
    }

    [HttpGet]
    public IActionResult Checkout(string? lang)
    {
        var language = LanguageExtensions.NormalizeLanguage(lang);
        if (!(User.Identity?.IsAuthenticated ?? false))
        {
            var returnUrl = Url.Action(nameof(Checkout), nameof(HomeController).Replace("Controller", string.Empty), new { lang = language }) ?? "/";
            return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl });
        }

        var cart = GetCart();
        if (cart.Count == 0)
        {
            return RedirectToAction(nameof(Cart), new { lang = language });
        }

        return View(new CheckoutViewModel
        {
            Language = language,
            Items = cart
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel model)
    {
        model.Language = LanguageExtensions.NormalizeLanguage(model.Language);
        if (!(User.Identity?.IsAuthenticated ?? false))
        {
            var returnUrl = Url.Action(nameof(Checkout), nameof(HomeController).Replace("Controller", string.Empty), new { lang = model.Language }) ?? "/";
            return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl });
        }

        model.Items = GetCart();
        if (model.Items.Count == 0)
        {
            ModelState.AddModelError(string.Empty, model.Language == "en"
                ? "Your cart is empty."
                : "Dein Warenkorb ist leer.");
        }
        var acceptTermsChecked = Request.Form["AcceptTerms"].Any(v => string.Equals(v, "true", StringComparison.OrdinalIgnoreCase) || v == "on");
        var acceptWithdrawalChecked = Request.Form["AcceptWithdrawal"].Any(v => string.Equals(v, "true", StringComparison.OrdinalIgnoreCase) || v == "on");

        model.AcceptTerms = acceptTermsChecked;
        model.AcceptWithdrawal = acceptWithdrawalChecked;

        if (!acceptTermsChecked)
        {
            ModelState.AddModelError(nameof(model.AcceptTerms), model.Language == "en"
                ? "Please read and accept the terms and conditions."
                : "Bitte AGB lesen und akzeptieren.");
        }
        if (!acceptWithdrawalChecked)
        {
            ModelState.AddModelError(nameof(model.AcceptWithdrawal), model.Language == "en"
                ? "Please read and accept the withdrawal policy."
                : "Bitte Widerrufsbelehrung lesen und akzeptieren.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _orderMailService.SendOrderRequestAsync(model, HttpContext.RequestAborted);
            SaveOrder(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Order request mail could not be sent for {Email}", model.Email);
            var prefix = model.Language == "en"
                ? "Order request email could not be sent"
                : "Bestellanfrage konnte per E-Mail nicht versendet werden";
            ModelState.AddModelError(string.Empty, $"{prefix}: {ex.Message}");
            return View(model);
        }

        SaveCart([]);
        model.Submitted = true;
        return View(model);
    }

    [HttpGet]
    public IActionResult Imprint(string? lang)
    {
        ViewData["Lang"] = LanguageExtensions.NormalizeLanguage(lang);
        return View();
    }

    [HttpGet]
    public IActionResult Privacy(string? lang)
    {
        ViewData["Lang"] = LanguageExtensions.NormalizeLanguage(lang);
        return View();
    }

    [HttpGet]
    public IActionResult Terms(string? lang)
    {
        ViewData["Lang"] = LanguageExtensions.NormalizeLanguage(lang);
        return View();
    }

    [HttpGet]
    public IActionResult Withdrawal(string? lang)
    {
        ViewData["Lang"] = LanguageExtensions.NormalizeLanguage(lang);
        return View();
    }

    [HttpGet]
    public IActionResult ShippingPayment(string? lang)
    {
        ViewData["Lang"] = LanguageExtensions.NormalizeLanguage(lang);
        return View();
    }

    [HttpGet]
    public IActionResult Orders(string? lang)
    {
        var language = LanguageExtensions.NormalizeLanguage(lang);
        if (!(User.Identity?.IsAuthenticated ?? false))
        {
            var returnUrl = Url.Action(nameof(Orders), "Home", new { lang = language }) ?? "/";
            return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl });
        }

        return View(new OrderHistoryViewModel
        {
            Language = language,
            Orders = ReadOrders()
                .OrderByDescending(x => x.CreatedUtc)
                .ToList()
        });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private List<CartItem> GetCart()
    {
        var userCartPath = GetUserCartPath();
        if (!string.IsNullOrWhiteSpace(userCartPath))
        {
            try
            {
                if (System.IO.File.Exists(userCartPath))
                {
                    var fileJson = System.IO.File.ReadAllText(userCartPath);
                    if (!string.IsNullOrWhiteSpace(fileJson))
                    {
                        return JsonSerializer.Deserialize<List<CartItem>>(fileJson) ?? [];
                    }
                }

                // One-time migration from session cart to user cart after login.
                var sessionJson = HttpContext.Session.GetString(CartSessionKey);
                if (!string.IsNullOrWhiteSpace(sessionJson))
                {
                    var migrated = JsonSerializer.Deserialize<List<CartItem>>(sessionJson) ?? [];
                    SaveCart(migrated);
                    HttpContext.Session.Remove(CartSessionKey);
                    return migrated;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not read persistent cart at {Path}", userCartPath);
            }

            return [];
        }

        var json = HttpContext.Session.GetString(CartSessionKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<CartItem>>(json) ?? [];
    }

    private void SaveCart(List<CartItem> cart)
    {
        var userCartPath = GetUserCartPath();
        if (!string.IsNullOrWhiteSpace(userCartPath))
        {
            try
            {
                var dir = Path.GetDirectoryName(userCartPath);
                if (!string.IsNullOrWhiteSpace(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                System.IO.File.WriteAllText(userCartPath, JsonSerializer.Serialize(cart));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not write persistent cart at {Path}", userCartPath);
            }

            return;
        }

        HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
    }

    private string? GetUserCartPath()
    {
        if (!(User.Identity?.IsAuthenticated ?? false))
        {
            return null;
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(userId)));
        return Path.Combine(_appDataPathProvider.DataDirectory, "carts", $"{hash}.json");
    }

    private string? GetUserOrdersPath()
    {
        if (!(User.Identity?.IsAuthenticated ?? false))
        {
            return null;
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(userId)));
        return Path.Combine(_appDataPathProvider.DataDirectory, "orders", $"{hash}.json");
    }

    private List<OrderRecord> ReadOrders()
    {
        var path = GetUserOrdersPath();
        if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
        {
            return [];
        }

        try
        {
            var json = System.IO.File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<OrderRecord>>(json) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not read order history at {Path}", path);
            return [];
        }
    }

    private void WriteOrders(List<OrderRecord> orders)
    {
        var path = GetUserOrdersPath();
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        try
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(dir))
            {
                Directory.CreateDirectory(dir);
            }

            System.IO.File.WriteAllText(path, JsonSerializer.Serialize(orders));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not write order history at {Path}", path);
        }
    }

    private void SaveOrder(CheckoutViewModel model)
    {
        var orders = ReadOrders();
        orders.Add(new OrderRecord
        {
            Id = Guid.NewGuid().ToString("N"),
            CreatedUtc = DateTime.UtcNow,
            FullName = model.FullName,
            Email = model.Email,
            Street = model.Street,
            PostalCode = model.PostalCode,
            City = model.City,
            NetTotal = model.NetTotal,
            VatAmount = model.VatAmount,
            GrossTotal = model.GrossTotal,
            Items = model.Items.Select(i => new CartItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                ProductDescription = i.ProductDescription,
                ProductImageUrl = i.ProductImageUrl,
                BasePrice = i.BasePrice,
                SelectedOptions = i.SelectedOptions.Select(o => new ShopOption
                {
                    Id = o.Id,
                    GroupName = o.GroupName,
                    NameDe = o.NameDe,
                    NameEn = o.NameEn,
                    DescriptionDe = o.DescriptionDe,
                    DescriptionEn = o.DescriptionEn,
                    ImageUrl = o.ImageUrl,
                    IsRequiredGroup = o.IsRequiredGroup,
                    PriceDelta = o.PriceDelta
                }).ToList()
            }).ToList()
        });

        WriteOrders(orders);
    }
}
