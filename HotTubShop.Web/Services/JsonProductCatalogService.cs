using System.Text.Json;
using HotTubShop.Web.Models;

namespace HotTubShop.Web.Services;

public class JsonProductCatalogService : IProductCatalogService
{
    private readonly string _catalogPath;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public JsonProductCatalogService(IWebHostEnvironment env)
    {
        var dataDir = Path.Combine(env.ContentRootPath, "App_Data");
        Directory.CreateDirectory(dataDir);
        _catalogPath = Path.Combine(dataDir, "catalog.json");

        if (!File.Exists(_catalogPath))
        {
            File.WriteAllText(_catalogPath, JsonSerializer.Serialize(CreateSeedData(), JsonOptions));
        }
    }

    public async Task<IReadOnlyList<HotTubProduct>> GetProductsAsync()
    {
        var items = await ReadAsync();
        return items.OrderBy(p => p.NameDe).ToList();
    }

    public async Task<HotTubProduct?> GetByIdAsync(string id)
    {
        var items = await ReadAsync();
        return items.FirstOrDefault(p => p.Id == id);
    }

    public async Task AddProductAsync(HotTubProduct product)
    {
        var items = await ReadAsync();
        product.Id = string.IsNullOrWhiteSpace(product.Id) ? Guid.NewGuid().ToString("N") : product.Id;
        items.Add(product);
        await WriteAsync(items);
    }

    public async Task UpdateProductAsync(HotTubProduct product)
    {
        var items = await ReadAsync();
        var index = items.FindIndex(p => p.Id == product.Id);
        if (index < 0)
        {
            return;
        }

        items[index] = product;
        await WriteAsync(items);
    }

    public async Task DeleteProductAsync(string id)
    {
        var items = await ReadAsync();
        items.RemoveAll(x => x.Id == id);
        await WriteAsync(items);
    }

    public async Task AddOptionAsync(string productId, ShopOption option)
    {
        var items = await ReadAsync();
        var product = items.FirstOrDefault(p => p.Id == productId);
        if (product is null)
        {
            return;
        }

        option.Id = string.IsNullOrWhiteSpace(option.Id) ? Guid.NewGuid().ToString("N") : option.Id;
        product.Options.Add(option);
        await WriteAsync(items);
    }

    public async Task UpdateOptionAsync(string productId, ShopOption option)
    {
        var items = await ReadAsync();
        var product = items.FirstOrDefault(p => p.Id == productId);
        if (product is null)
        {
            return;
        }

        var index = product.Options.FindIndex(o => o.Id == option.Id);
        if (index < 0)
        {
            return;
        }

        product.Options[index] = option;
        await WriteAsync(items);
    }

    public async Task DeleteOptionAsync(string productId, string optionId)
    {
        var items = await ReadAsync();
        var product = items.FirstOrDefault(p => p.Id == productId);
        if (product is null)
        {
            return;
        }

        product.Options.RemoveAll(o => o.Id == optionId);
        await WriteAsync(items);
    }

    private async Task<List<HotTubProduct>> ReadAsync()
    {
        await using var stream = File.OpenRead(_catalogPath);
        var products = await JsonSerializer.DeserializeAsync<List<HotTubProduct>>(stream);
        return products ?? [];
    }

    private async Task WriteAsync(List<HotTubProduct> products)
    {
        await using var stream = File.Create(_catalogPath);
        await JsonSerializer.SerializeAsync(stream, products, JsonOptions);
    }

    private static List<HotTubProduct> CreateSeedData()
    {
        return
        [
            new HotTubProduct
            {
                Id = "arctic-zen",
                Sku = "HT-ARCTIC-001",
                NameDe = "Arctic Zen 5",
                NameEn = "Arctic Zen 5",
                DescriptionDe = "Kompakter Premium-Whirlpool mit nordischem Holz-Look für bis zu 5 Personen.",
                DescriptionEn = "Compact premium hot tub with a Nordic timber look for up to 5 people.",
                ImageUrl = "https://images.unsplash.com/photo-1621535334652-17dbe17f9b5f?auto=format&fit=crop&w=1400&q=80",
                BasePrice = 7490m,
                Options =
                [
                    new ShopOption { Id = "heater-plus", GroupName = "Heizung", NameDe = "Schnellheizer 6kW", NameEn = "Fast heater 6kW", PriceDelta = 690m },
                    new ShopOption { Id = "lights-ice", GroupName = "Atmosphaere", NameDe = "LED Nordlicht Paket", NameEn = "Nordic lights package", PriceDelta = 420m },
                    new ShopOption { Id = "cover-premium", GroupName = "Abdeckung", NameDe = "Premium Thermo-Cover", NameEn = "Premium thermal cover", PriceDelta = 350m }
                ]
            },
            new HotTubProduct
            {
                Id = "fjord-lounge",
                Sku = "HT-FJORD-007",
                NameDe = "Fjord Lounge 7",
                NameEn = "Fjord Lounge 7",
                DescriptionDe = "Groesse fuer die ganze Familie mit extra Liegeplatz und leisem Zirkulationssystem.",
                DescriptionEn = "Family-sized comfort with a dedicated lounger and low-noise circulation.",
                ImageUrl = "https://images.unsplash.com/photo-1575429198097-0414ec08e8cd?auto=format&fit=crop&w=1400&q=80",
                BasePrice = 10390m,
                Options =
                [
                    new ShopOption { Id = "audio", GroupName = "Entertainment", NameDe = "Bluetooth Audio", NameEn = "Bluetooth audio", PriceDelta = 490m },
                    new ShopOption { Id = "steps", GroupName = "Komfort", NameDe = "Nordic Einstiegstreppe", NameEn = "Nordic access steps", PriceDelta = 210m },
                    new ShopOption { Id = "salt", GroupName = "Wasserpflege", NameDe = "Salzwasser-System", NameEn = "Salt water system", PriceDelta = 990m }
                ]
            }
        ];
    }
}
