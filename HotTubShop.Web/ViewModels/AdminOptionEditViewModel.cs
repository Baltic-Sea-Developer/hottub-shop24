using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HotTubShop.Web.ViewModels;

public class AdminOptionEditViewModel
{
    public string ProductId { get; set; } = string.Empty;
    public string? Id { get; set; }
    public List<string> ExistingGroups { get; set; } = [];
    public string? SelectedGroup { get; set; }
    public string? NewGroupName { get; set; }

    [Required]
    [Display(Name = "Gruppe")]
    public string GroupName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Name (Deutsch)")]
    public string NameDe { get; set; } = string.Empty;

    [Display(Name = "Name (English)")]
    public string? NameEn { get; set; }

    [Display(Name = "Bild URL")]
    public string? ImageUrl { get; set; }

    [Display(Name = "Bild Upload")]
    public IFormFile? ImageFile { get; set; }

    [Required]
    [Display(Name = "Preisaufschlag")]
    public string PriceDelta { get; set; } = string.Empty;
}
