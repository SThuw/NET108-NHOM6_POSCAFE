using CafePOSManagement.Web.Models.ViewModels;
using CafePOSManagement.Web.Services.Interfaces;

namespace CafePOSManagement.Web.Services.Implementations;

public sealed class PosService(TimeProvider timeProvider, IProductService products, IOrderService orders) : IPosService
{
    private static readonly TimeZoneInfo VietnamTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");

    public PosIndexViewModel GetOverview()
    {
        var localTime = TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), VietnamTimeZone);
        return new PosIndexViewModel(localTime, products.GetCategories(), products.GetProducts(),
            orders.GetDiscountCodes(), orders.GetTaxRates(), orders.GetNextOrderNumber());
    }
}
