using System.ComponentModel.DataAnnotations;

namespace HotTubShop.Web.ViewModels;

public class AdminProductEditViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required]
    public string Sku { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Name (Deutsch)")]
    public string NameDe { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Name (English)")]
    public string NameEn { get; set; } = string.Empty;

    [Display(Name = "Beschreibung (Deutsch)")]
    public string DescriptionDe { get; set; } = string.Empty;

    [Display(Name = "Description (English)")]
    public string DescriptionEn { get; set; } = string.Empty;

    [Display(Name = "Bild URL")]
    public string ImageUrl { get; set; } = string.Empty;

    [Range(0, 999999)]
    [Display(Name = "Basispreis")]
    public decimal BasePrice { get; set; }
}
