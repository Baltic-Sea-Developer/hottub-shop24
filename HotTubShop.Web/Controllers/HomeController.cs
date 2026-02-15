using System.Diagnostics;
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

    public HomeController(ILogger<HomeController> logger, IProductCatalogService catalogService)
    {
        _logger = logger;
        _catalogService = catalogService;
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
            ProductImageUrl = product.ImageUrl,
            BasePrice = product.BasePrice,
            SelectedOptions = selectedOptions
        });
        SaveCart(cart);

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
        if (GetCart().Count == 0)
        {
            return RedirectToAction(nameof(Cart), new { lang = LanguageExtensions.NormalizeLanguage(lang) });
        }

        return View(new CheckoutViewModel { Language = LanguageExtensions.NormalizeLanguage(lang) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Checkout(CheckoutViewModel model)
    {
        model.Language = LanguageExtensions.NormalizeLanguage(model.Language);
        if (GetCart().Count == 0)
        {
            ModelState.AddModelError(string.Empty, model.Language == "en"
                ? "Your cart is empty."
                : "Dein Warenkorb ist leer.");
        }

        if (!ModelState.IsValid)
        {
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

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private List<CartItem> GetCart()
    {
        var json = HttpContext.Session.GetString(CartSessionKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<CartItem>>(json) ?? [];
    }

    private void SaveCart(List<CartItem> cart)
    {
        HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
    }
}
