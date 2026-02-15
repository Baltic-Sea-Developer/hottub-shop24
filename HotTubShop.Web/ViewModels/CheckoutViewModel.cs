using System.ComponentModel.DataAnnotations;
using HotTubShop.Web.Models;

namespace HotTubShop.Web.ViewModels;

public class CheckoutViewModel
{
    private const decimal VatRate = 0.19m;

    public string Language { get; set; } = "de";
    public List<CartItem> Items { get; set; } = [];
    public decimal GrossTotal => Items.Sum(i => i.TotalPrice);
    public decimal NetTotal => Math.Round(GrossTotal / (1m + VatRate), 2, MidpointRounding.AwayFromZero);
    public decimal VatAmount => Math.Round(GrossTotal - NetTotal, 2, MidpointRounding.AwayFromZero);

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

    [Display(Name = "AGB gelesen und akzeptiert")]
    public bool AcceptTerms { get; set; }

    [Display(Name = "Widerrufsbelehrung gelesen und akzeptiert")]
    public bool AcceptWithdrawal { get; set; }

    public bool Submitted { get; set; }
}
