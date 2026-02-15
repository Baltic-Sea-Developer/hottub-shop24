using HotTubShop.Web.Models;

namespace HotTubShop.Web.ViewModels;

public class CartViewModel
{
    public string Language { get; set; } = "de";
    public List<CartItem> Items { get; set; } = [];
    public decimal Total => Items.Sum(i => i.TotalPrice);
}
