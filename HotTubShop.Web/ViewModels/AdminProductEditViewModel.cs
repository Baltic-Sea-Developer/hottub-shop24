using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HotTubShop.Web.ViewModels;

public class AdminProductEditViewModel
{
    public string? Id { get; set; }

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

    [Display(Name = "Bild Upload")]
    public IFormFile? ImageFile { get; set; }

    [Display(Name = "Weitere Bild-URLs (eine pro Zeile)")]
    public string? GalleryImageUrlsText { get; set; }

    [Display(Name = "Weitere Bilder Upload")]
    public List<IFormFile>? GalleryImageFiles { get; set; }

    [Required]
    [Display(Name = "Basispreis")]
    public string BasePrice { get; set; } = string.Empty;
}
