using System.Diagnostics;
using HotTubShop.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using HotTubShop.Web.Models;
using HotTubShop.Web.Services;
using HotTubShop.Web.ViewModels;

namespace HotTubShop.Web.Controllers;

public class HomeController : Controller
{
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
            Product = product
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

        var selectedOptions = product.Options
            .Where(o => model.SelectedOptionIds.Contains(o.Id))
            .ToList();

        model.Language = LanguageExtensions.NormalizeLanguage(model.Language);
        model.Product = product;
        model.Configured = new ConfiguredHotTub
        {
            Product = product,
            SelectedOptions = selectedOptions
        };

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
}
