using HotTubShop.Web.Models;

namespace HotTubShop.Web.ViewModels;

public class OrderHistoryViewModel
{
    public string Language { get; set; } = "de";
    public List<OrderRecord> Orders { get; set; } = [];
}
