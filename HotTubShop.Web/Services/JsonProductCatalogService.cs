using HotTubShop.Web.Infrastructure;
using System.Text.Json;
using HotTubShop.Web.Models;

namespace HotTubShop.Web.Services;

public class JsonProductCatalogService : IProductCatalogService
{
    private readonly string _catalogPath;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public JsonProductCatalogService(AppDataPathProvider dataPathProvider)
    {
        var dataDir = dataPathProvider.DataDirectory;
        Directory.CreateDirectory(dataDir);
        _catalogPath = Path.Combine(dataDir, "catalog.json");

        if (!File.Exists(_catalogPath))
        {
            File.WriteAllText(_catalogPath, JsonSerializer.Serialize(new List<HotTubProduct>(), JsonOptions));
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

}
