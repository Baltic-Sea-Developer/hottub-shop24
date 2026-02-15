using System.ComponentModel.DataAnnotations;

namespace HotTubShop.Web.ViewModels;

public class CheckoutViewModel
{
    public string Language { get; set; } = "de";

    [Required]
    [Display(Name = "Vor- und Nachname")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "E-Mail")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Straße und Hausnummer")]
    public string Street { get; set; } = string.Empty;

    [Required]
    [Display(Name = "PLZ")]
    public string PostalCode { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Ort")]
    public string City { get; set; } = string.Empty;

    public bool Submitted { get; set; }
}
