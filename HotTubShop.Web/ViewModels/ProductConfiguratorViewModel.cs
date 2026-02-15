using HotTubShop.Web.Models;

namespace HotTubShop.Web.ViewModels;

public class ProductConfiguratorViewModel
{
    public string Language { get; set; } = "de";
    public HotTubProduct Product { get; set; } = new();
    public Dictionary<string, string> SelectedByGroup { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public ConfiguredHotTub? Configured { get; set; }
}
