using CafePOSManagement.Web.Models.Entities;
using CafePOSManagement.Web.Models.ViewModels;

namespace CafePOSManagement.Web.Services.Interfaces;

public interface IOrderService
{
    IReadOnlyList<DiscountCode> GetDiscountCodes();
    IReadOnlyList<decimal> GetTaxRates();
    string GetNextOrderNumber();
    Order Preview(CheckoutOrderDto input);
    Order Checkout(CheckoutOrderDto input);
}
