using CafePOSManagement.Web.Models.Entities;

namespace CafePOSManagement.Web.Models.ViewModels;

public sealed record PosIndexViewModel(DateTimeOffset CurrentTime, IReadOnlyList<Category> Categories,
    IReadOnlyList<Product> Products, IReadOnlyList<DiscountCode> Discounts,
    IReadOnlyList<decimal> TaxRates, string NextOrderNumber);
