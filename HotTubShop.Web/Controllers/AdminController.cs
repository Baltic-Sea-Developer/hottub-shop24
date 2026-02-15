using HotTubShop.Web.Models;
using HotTubShop.Web.Services;
using HotTubShop.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace HotTubShop.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IProductCatalogService _catalogService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IProductCatalogService catalogService, ILogger<AdminController> logger)
    {
        _catalogService = catalogService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _catalogService.GetProductsAsync();
        return View(products);
    }

    [HttpGet]
    public IActionResult CreateProduct() => View(new AdminProductEditViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProduct(AdminProductEditViewModel model)
    {
        ModelState.Remove(nameof(model.Id));
        ModelState.Remove(nameof(model.NameEn));
        ModelState.Remove(nameof(model.DescriptionDe));
        ModelState.Remove(nameof(model.DescriptionEn));
        ModelState.Remove(nameof(model.ImageUrl));

        if (!TryParseMoney(model.BasePrice, out var basePrice))
        {
            ModelState.AddModelError(nameof(model.BasePrice), "Bitte einen gültigen Preis eingeben (z. B. 19999,00).");
        }

        if (!ModelState.IsValid)
        {
            TempData["AdminError"] = JoinModelStateErrors();
            return View(model);
        }

        try
        {
            await _catalogService.AddProductAsync(new HotTubProduct
            {
                Id = Guid.NewGuid().ToString("N"),
                Sku = model.Sku,
                NameDe = model.NameDe,
                NameEn = string.IsNullOrWhiteSpace(model.NameEn) ? model.NameDe : model.NameEn,
                DescriptionDe = model.DescriptionDe ?? string.Empty,
                DescriptionEn = string.IsNullOrWhiteSpace(model.DescriptionEn) ? (model.DescriptionDe ?? string.Empty) : model.DescriptionEn,
                ImageUrl = model.ImageUrl ?? string.Empty,
                BasePrice = basePrice
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add product {Sku}", model.Sku);
            ModelState.AddModelError(string.Empty, $"Speichern fehlgeschlagen: {ex.Message}");
            TempData["AdminError"] = ex.Message;
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> EditProduct(string id)
    {
        var product = await _catalogService.GetByIdAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        return View(new AdminProductEditViewModel
        {
            Id = product.Id,
            Sku = product.Sku,
            NameDe = product.NameDe,
            NameEn = product.NameEn,
            DescriptionDe = product.DescriptionDe,
            DescriptionEn = product.DescriptionEn,
            ImageUrl = product.ImageUrl,
            BasePrice = product.BasePrice.ToString("0.##", CultureInfo.CurrentCulture)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProduct(AdminProductEditViewModel model)
    {
        ModelState.Remove(nameof(model.NameEn));
        ModelState.Remove(nameof(model.DescriptionDe));
        ModelState.Remove(nameof(model.DescriptionEn));
        ModelState.Remove(nameof(model.ImageUrl));

        if (!TryParseMoney(model.BasePrice, out var basePrice))
        {
            ModelState.AddModelError(nameof(model.BasePrice), "Bitte einen gültigen Preis eingeben (z. B. 19999,00).");
        }

        if (!ModelState.IsValid)
        {
            TempData["AdminError"] = JoinModelStateErrors();
            return View(model);
        }

        var existing = await _catalogService.GetByIdAsync(model.Id);
        if (existing is null)
        {
            return NotFound();
        }

        existing.Sku = model.Sku;
        existing.NameDe = model.NameDe;
        existing.NameEn = string.IsNullOrWhiteSpace(model.NameEn) ? model.NameDe : model.NameEn;
        existing.DescriptionDe = model.DescriptionDe;
        existing.DescriptionEn = string.IsNullOrWhiteSpace(model.DescriptionEn) ? model.DescriptionDe : model.DescriptionEn;
        existing.ImageUrl = model.ImageUrl;
        existing.BasePrice = basePrice;

        try
        {
            await _catalogService.UpdateProductAsync(existing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update product {Id}", model.Id);
            ModelState.AddModelError(string.Empty, $"Speichern fehlgeschlagen: {ex.Message}");
            TempData["AdminError"] = ex.Message;
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProduct(string id)
    {
        await _catalogService.DeleteProductAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> ProductOptions(string id)
    {
        var product = await _catalogService.GetByIdAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        return View(product);
    }

    [HttpGet]
    public async Task<IActionResult> AddOption(string productId)
    {
        var model = new AdminOptionEditViewModel { ProductId = productId };
        await PopulateOptionGroups(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddOption(AdminOptionEditViewModel model)
    {
        ModelState.Remove(nameof(model.Id));
        ModelState.Remove(nameof(model.NameEn));
        ModelState.Remove(nameof(model.GroupName));
        model.GroupName = ResolveGroupName(model);
        if (string.IsNullOrWhiteSpace(model.GroupName))
        {
            ModelState.AddModelError(nameof(model.GroupName), "Bitte bestehende Gruppe wählen oder neue Gruppe eingeben.");
        }

        if (!TryParseMoney(model.PriceDelta, out var priceDelta))
        {
            ModelState.AddModelError(nameof(model.PriceDelta), "Bitte einen gültigen Aufpreis eingeben (z. B. 499,00).");
        }

        if (!ModelState.IsValid)
        {
            await PopulateOptionGroups(model);
            TempData["AdminError"] = JoinModelStateErrors();
            return View(model);
        }

        try
        {
            await _catalogService.AddOptionAsync(model.ProductId, new ShopOption
            {
                Id = Guid.NewGuid().ToString("N"),
                GroupName = model.GroupName,
                NameDe = model.NameDe,
                NameEn = string.IsNullOrWhiteSpace(model.NameEn) ? model.NameDe : model.NameEn,
                ImageUrl = model.ImageUrl ?? string.Empty,
                PriceDelta = priceDelta
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add option for product {ProductId}", model.ProductId);
            ModelState.AddModelError(string.Empty, $"Speichern fehlgeschlagen: {ex.Message}");
            TempData["AdminError"] = ex.Message;
            return View(model);
        }

        return RedirectToAction(nameof(ProductOptions), new { id = model.ProductId });
    }

    [HttpGet]
    public async Task<IActionResult> EditOption(string productId, string optionId)
    {
        var product = await _catalogService.GetByIdAsync(productId);
        var option = product?.Options.FirstOrDefault(o => o.Id == optionId);
        if (product is null || option is null)
        {
            return NotFound();
        }

        var model = new AdminOptionEditViewModel
        {
            ProductId = productId,
            Id = option.Id,
            GroupName = option.GroupName,
            SelectedGroup = option.GroupName,
            NameDe = option.NameDe,
            NameEn = option.NameEn,
            ImageUrl = option.ImageUrl,
            PriceDelta = option.PriceDelta.ToString("0.##", CultureInfo.CurrentCulture)
        };
        await PopulateOptionGroups(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditOption(AdminOptionEditViewModel model)
    {
        ModelState.Remove(nameof(model.NameEn));
        ModelState.Remove(nameof(model.GroupName));
        model.GroupName = ResolveGroupName(model);
        if (string.IsNullOrWhiteSpace(model.GroupName))
        {
            ModelState.AddModelError(nameof(model.GroupName), "Bitte bestehende Gruppe wählen oder neue Gruppe eingeben.");
        }

        if (!TryParseMoney(model.PriceDelta, out var priceDelta))
        {
            ModelState.AddModelError(nameof(model.PriceDelta), "Bitte einen gültigen Aufpreis eingeben (z. B. 499,00).");
        }

        if (!ModelState.IsValid)
        {
            await PopulateOptionGroups(model);
            TempData["AdminError"] = JoinModelStateErrors();
            return View(model);
        }

        try
        {
            await _catalogService.UpdateOptionAsync(model.ProductId, new ShopOption
            {
                Id = model.Id,
                GroupName = model.GroupName,
                NameDe = model.NameDe,
                NameEn = string.IsNullOrWhiteSpace(model.NameEn) ? model.NameDe : model.NameEn,
                ImageUrl = model.ImageUrl ?? string.Empty,
                PriceDelta = priceDelta
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update option {OptionId} for product {ProductId}", model.Id, model.ProductId);
            ModelState.AddModelError(string.Empty, $"Speichern fehlgeschlagen: {ex.Message}");
            TempData["AdminError"] = ex.Message;
            return View(model);
        }

        return RedirectToAction(nameof(ProductOptions), new { id = model.ProductId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteOption(string productId, string optionId)
    {
        await _catalogService.DeleteOptionAsync(productId, optionId);
        return RedirectToAction(nameof(ProductOptions), new { id = productId });
    }

    private static bool TryParseMoney(string? value, out decimal result)
    {
        var normalized = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            result = 0m;
            return false;
        }

        normalized = normalized.Replace(" ", string.Empty);
        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.CurrentCulture, out result)
            || decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.GetCultureInfo("de-DE"), out result)
            || decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
    }

    private string JoinModelStateErrors()
    {
        var errors = ModelState
            .SelectMany(kvp => kvp.Value?.Errors.Select(e => $"{kvp.Key}: {e.ErrorMessage}") ?? [])
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .ToList();

        var message = errors.Count == 0 ? "Unbekannter Validierungsfehler." : string.Join(" | ", errors);
        _logger.LogWarning("Admin form validation failed: {Message}", message);
        return message;
    }

    private static string ResolveGroupName(AdminOptionEditViewModel model)
    {
        return string.IsNullOrWhiteSpace(model.NewGroupName)
            ? (model.SelectedGroup ?? model.GroupName ?? string.Empty).Trim()
            : model.NewGroupName.Trim();
    }

    private async Task PopulateOptionGroups(AdminOptionEditViewModel model)
    {
        var product = await _catalogService.GetByIdAsync(model.ProductId);
        model.ExistingGroups = product?.Options
            .Select(o => o.GroupName)
            .Where(g => !string.IsNullOrWhiteSpace(g))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => g)
            .ToList() ?? [];
    }
}
