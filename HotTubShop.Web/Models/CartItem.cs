namespace HotTubShop.Web.Models;

public class CartItem
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string ProductDescription { get; set; } = string.Empty;
    public string ProductImageUrl { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public List<ShopOption> SelectedOptions { get; set; } = [];
    public decimal TotalPrice => BasePrice + SelectedOptions.Sum(o => o.PriceDelta);
}
