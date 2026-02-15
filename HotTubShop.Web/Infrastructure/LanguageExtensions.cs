using HotTubShop.Web.Models;

namespace HotTubShop.Web.Infrastructure;

public static class LanguageExtensions
{
    public static string NormalizeLanguage(string? lang)
    {
        if (string.IsNullOrWhiteSpace(lang))
        {
            return "de";
        }

        return lang.StartsWith("en", StringComparison.OrdinalIgnoreCase) ? "en" : "de";
    }

    public static string LocalizedName(this HotTubProduct product, string lang) =>
        lang == "en" && !string.IsNullOrWhiteSpace(product.NameEn) ? product.NameEn : product.NameDe;

    public static string LocalizedDescription(this HotTubProduct product, string lang) =>
        lang == "en" && !string.IsNullOrWhiteSpace(product.DescriptionEn) ? product.DescriptionEn : product.DescriptionDe;

    public static string LocalizedName(this ShopOption option, string lang) =>
        lang == "en" && !string.IsNullOrWhiteSpace(option.NameEn) ? option.NameEn : option.NameDe;

    public static string LocalizedDescription(this ShopOption option, string lang) =>
        lang == "en" && !string.IsNullOrWhiteSpace(option.DescriptionEn) ? option.DescriptionEn : option.DescriptionDe;
}
