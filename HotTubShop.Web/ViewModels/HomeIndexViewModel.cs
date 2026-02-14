using HotTubShop.Web.Models;

namespace HotTubShop.Web.ViewModels;

public class HomeIndexViewModel
{
    public string Language { get; set; } = "de";
    public IReadOnlyList<HotTubProduct> Products { get; set; } = [];
}
