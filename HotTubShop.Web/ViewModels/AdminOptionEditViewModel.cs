using System.ComponentModel.DataAnnotations;

namespace HotTubShop.Web.ViewModels;

public class AdminOptionEditViewModel
{
    public string ProductId { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Gruppe")]
    public string GroupName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Name (Deutsch)")]
    public string NameDe { get; set; } = string.Empty;

    [Display(Name = "Name (English)")]
    public string? NameEn { get; set; }

    [Required]
    [Display(Name = "Preisaufschlag")]
    public string PriceDelta { get; set; } = string.Empty;
}
