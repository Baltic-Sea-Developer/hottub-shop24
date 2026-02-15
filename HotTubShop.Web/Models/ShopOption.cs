namespace HotTubShop.Web.Models;

public class ShopOption
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string GroupName { get; set; } = string.Empty;
    public string NameDe { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal PriceDelta { get; set; }
}
