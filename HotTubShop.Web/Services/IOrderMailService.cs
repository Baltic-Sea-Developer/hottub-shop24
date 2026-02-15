using HotTubShop.Web.ViewModels;

namespace HotTubShop.Web.Services;

public interface IOrderMailService
{
    Task SendOrderRequestAsync(CheckoutViewModel model, CancellationToken cancellationToken = default);
}
