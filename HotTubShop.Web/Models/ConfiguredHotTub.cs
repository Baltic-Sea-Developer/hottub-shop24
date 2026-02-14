namespace HotTubShop.Web.Models;

public class ConfiguredHotTub
{
    public required HotTubProduct Product { get; init; }
    public List<ShopOption> SelectedOptions { get; init; } = [];

    public decimal TotalPrice => Product.BasePrice + SelectedOptions.Sum(o => o.PriceDelta);
}
