using HotTubShop.Web.Models;
using HotTubShop.Web.Services;
using HotTubShop.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotTubShop.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IProductCatalogService _catalogService;

    public AdminController(IProductCatalogService catalogService)
    {
        _catalogService = catalogService;
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
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _catalogService.AddProductAsync(new HotTubProduct
        {
            Sku = model.Sku,
            NameDe = model.NameDe,
            NameEn = model.NameEn,
            DescriptionDe = model.DescriptionDe,
            DescriptionEn = model.DescriptionEn,
            ImageUrl = model.ImageUrl,
            BasePrice = model.BasePrice
        });

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
            BasePrice = product.BasePrice
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProduct(AdminProductEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existing = await _catalogService.GetByIdAsync(model.Id);
        if (existing is null)
        {
            return NotFound();
        }

        existing.Sku = model.Sku;
        existing.NameDe = model.NameDe;
        existing.NameEn = model.NameEn;
        existing.DescriptionDe = model.DescriptionDe;
        existing.DescriptionEn = model.DescriptionEn;
        existing.ImageUrl = model.ImageUrl;
        existing.BasePrice = model.BasePrice;

        await _catalogService.UpdateProductAsync(existing);
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
    public IActionResult AddOption(string productId)
    {
        return View(new AdminOptionEditViewModel { ProductId = productId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddOption(AdminOptionEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _catalogService.AddOptionAsync(model.ProductId, new ShopOption
        {
            GroupName = model.GroupName,
            NameDe = model.NameDe,
            NameEn = model.NameEn,
            PriceDelta = model.PriceDelta
        });

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

        return View(new AdminOptionEditViewModel
        {
            ProductId = productId,
            Id = option.Id,
            GroupName = option.GroupName,
            NameDe = option.NameDe,
            NameEn = option.NameEn,
            PriceDelta = option.PriceDelta
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditOption(AdminOptionEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _catalogService.UpdateOptionAsync(model.ProductId, new ShopOption
        {
            Id = model.Id,
            GroupName = model.GroupName,
            NameDe = model.NameDe,
            NameEn = model.NameEn,
            PriceDelta = model.PriceDelta
        });

        return RedirectToAction(nameof(ProductOptions), new { id = model.ProductId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteOption(string productId, string optionId)
    {
        await _catalogService.DeleteOptionAsync(productId, optionId);
        return RedirectToAction(nameof(ProductOptions), new { id = productId });
    }
}
