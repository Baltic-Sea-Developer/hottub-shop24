using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Options;
using HotTubShop.Web.ViewModels;
using HotTubShop.Web.Infrastructure;

namespace HotTubShop.Web.Services;

public class SmtpOrderMailService : IOrderMailService
{
    private readonly OrderMailOptions _options;

    public SmtpOrderMailService(IOptions<OrderMailOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendOrderRequestAsync(CheckoutViewModel model, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.Host) || string.IsNullOrWhiteSpace(_options.FromAddress))
        {
            throw new InvalidOperationException("E-Mail-Versand ist nicht konfiguriert (Mail:Smtp Host/FromAddress).");
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromAddress, _options.FromName),
            Subject = model.Language == "en" ? "Your order request at hottub-shop24" : "Ihre Bestellanfrage bei hottub-shop24",
            Body = BuildMailBody(model),
            IsBodyHtml = false
        };
        message.To.Add(model.Email);

        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl
        };

        if (!string.IsNullOrWhiteSpace(_options.UserName))
        {
            client.Credentials = new NetworkCredential(_options.UserName, _options.Password);
        }
        else
        {
            client.UseDefaultCredentials = true;
        }

        using var ctr = cancellationToken.Register(() => client.SendAsyncCancel());
        await client.SendMailAsync(message);
    }

    private static string BuildMailBody(CheckoutViewModel model)
    {
        var isEn = model.Language == "en";
        var sb = new StringBuilder();
        sb.AppendLine(isEn ? "Thank you for your order request." : "Vielen Dank für Ihre Bestellanfrage.");
        sb.AppendLine();
        sb.AppendLine(isEn ? "Order summary:" : "Bestellzusammenfassung:");
        sb.AppendLine(new string('-', 60));

        foreach (var item in model.Items)
        {
            sb.AppendLine(item.ProductName);
            if (!string.IsNullOrWhiteSpace(item.ProductDescription))
            {
                sb.AppendLine(StripHtml(item.ProductDescription));
            }
            sb.AppendLine($"{(isEn ? "Base price" : "Basispreis")}: {item.BasePrice:N2} EUR");

            foreach (var option in item.SelectedOptions)
            {
                sb.AppendLine($"  - {option.LocalizedName(model.Language)} (+{option.PriceDelta:N2} EUR)");
                var optionDescription = option.LocalizedDescription(model.Language);
                if (!string.IsNullOrWhiteSpace(optionDescription))
                {
                    sb.AppendLine($"    {StripHtml(optionDescription)}");
                }
            }

            sb.AppendLine($"{(isEn ? "Item total" : "Positionssumme")}: {item.TotalPrice:N2} EUR");
            sb.AppendLine();
        }

        sb.AppendLine($"{(isEn ? "Net total" : "Netto")}: {model.NetTotal:N2} EUR");
        sb.AppendLine($"{(isEn ? "VAT (19%)" : "MwSt. (19%)")}: {model.VatAmount:N2} EUR");
        sb.AppendLine($"{(isEn ? "Gross total" : "Brutto")}: {model.GrossTotal:N2} EUR");
        sb.AppendLine();
        sb.AppendLine(isEn ? "Customer data:" : "Kundendaten:");
        sb.AppendLine($"{model.FullName}");
        sb.AppendLine($"{model.Street}");
        sb.AppendLine($"{model.PostalCode} {model.City}");
        sb.AppendLine($"{model.Email}");

        return sb.ToString();
    }

    private static string StripHtml(string input)
    {
        var array = new char[input.Length];
        var idx = 0;
        var inside = false;
        foreach (var c in input)
        {
            if (c == '<')
            {
                inside = true;
                continue;
            }

            if (c == '>')
            {
                inside = false;
                continue;
            }

            if (!inside)
            {
                array[idx++] = c;
            }
        }

        return new string(array, 0, idx).Trim();
    }
}
