using HotTubShop.Web.Models;

namespace HotTubShop.Web.ViewModels;

public class ProductConfiguratorViewModel
{
    public string Language { get; set; } = "de";
    public HotTubProduct Product { get; set; } = new();
    public List<string> SelectedOptionIds { get; set; } = [];
    public ConfiguredHotTub? Configured { get; set; }
}
