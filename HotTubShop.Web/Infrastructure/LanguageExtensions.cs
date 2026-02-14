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
        lang == "en" ? product.NameEn : product.NameDe;

    public static string LocalizedDescription(this HotTubProduct product, string lang) =>
        lang == "en" ? product.DescriptionEn : product.DescriptionDe;

    public static string LocalizedName(this ShopOption option, string lang) =>
        lang == "en" ? option.NameEn : option.NameDe;
}
