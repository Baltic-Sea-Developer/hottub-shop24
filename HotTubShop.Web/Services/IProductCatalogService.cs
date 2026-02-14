using HotTubShop.Web.Models;

namespace HotTubShop.Web.Services;

public interface IProductCatalogService
{
    Task<IReadOnlyList<HotTubProduct>> GetProductsAsync();
    Task<HotTubProduct?> GetByIdAsync(string id);
    Task AddProductAsync(HotTubProduct product);
    Task UpdateProductAsync(HotTubProduct product);
    Task DeleteProductAsync(string id);
    Task AddOptionAsync(string productId, ShopOption option);
    Task UpdateOptionAsync(string productId, ShopOption option);
    Task DeleteOptionAsync(string productId, string optionId);
}
