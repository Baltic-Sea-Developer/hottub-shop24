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

    [Display(Name = "Name (English)")]
    public string? NameEn { get; set; }

    [Display(Name = "Beschreibung (Deutsch)")]
    public string? DescriptionDe { get; set; }

    [Display(Name = "Description (English)")]
    public string? DescriptionEn { get; set; }

    [Display(Name = "Bild URL")]
    public string? ImageUrl { get; set; }

    [Required]
    [Display(Name = "Basispreis")]
    public string BasePrice { get; set; } = string.Empty;
}
