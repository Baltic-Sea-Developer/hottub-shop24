namespace HotTubShop.Web.Models;

public class HotTubProduct
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Sku { get; set; } = string.Empty;
    public string NameDe { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string DescriptionDe { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public List<ShopOption> Options { get; set; } = [];
}
