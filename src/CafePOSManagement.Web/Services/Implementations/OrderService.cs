using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using CafePOSManagement.Web.Models.Entities;
using CafePOSManagement.Web.Models.ViewModels;
using CafePOSManagement.Web.Services.Interfaces;

namespace CafePOSManagement.Web.Services.Implementations;

public sealed class OrderService(IProductService products, TimeProvider clock) : IOrderService
{
    private static readonly object Gate = new();
    private static readonly DiscountCode[] Discounts = [new("SALE10", "Giảm 10%", 10), new("SALE20", "Giảm 20%", 20)];
    private static readonly decimal[] TaxRates = [0, 5, 8, 10];
    // shortcut: Đơn và khóa chống gửi trùng mất khi restart; chuyển sang transaction + unique key khi dùng SQL Server.
    private static readonly List<(Order Order, string Fingerprint)> Orders = [];
    private static int sequence = 3;

    public IReadOnlyList<DiscountCode> GetDiscountCodes() => Array.AsReadOnly(Discounts);
    public IReadOnlyList<decimal> GetTaxRates() => Array.AsReadOnly(TaxRates);
    public string GetNextOrderNumber() { lock (Gate) return $"#{sequence + 1:000}"; }

    public Order Preview(CheckoutOrderDto input)
    {
        Validate(input);
        return Calculate(input) with { OrderNumber = GetNextOrderNumber() };
    }

    public Order Checkout(CheckoutOrderDto input)
    {
        Validate(input);
        var fingerprint = JsonSerializer.Serialize(input);
        lock (Gate)
        {
            var previous = Orders.FirstOrDefault(entry => entry.Order.RequestId == input.RequestId);
            if (previous.Order is not null)
            {
                if (previous.Fingerprint != fingerprint)
                    throw new ValidationException("Mã yêu cầu đã được dùng cho một nội dung đơn khác. Vui lòng tạo đơn mới.");
                return previous.Order;
            }
            var order = Calculate(input) with { OrderNumber = $"#{++sequence:000}" };
            Orders.Add((order, fingerprint));
            return order;
        }
    }

    private static void Validate(CheckoutOrderDto input)
    {
        Validator.ValidateObject(input, new ValidationContext(input), true);
        foreach (var item in input.Items)
            Validator.ValidateObject(item, new ValidationContext(item), true);
        if (input.Items.Select(item => item.ProductId).Distinct().Count() != input.Items.Count)
            throw new ValidationException("Mỗi món chỉ được xuất hiện một dòng trong đơn.");
        if (!TaxRates.Contains(input.TaxPercent))
            throw new ValidationException("Mức VAT chưa được cấu hình.");
    }

    private Order Calculate(CheckoutOrderDto input)
    {
        var discount = string.IsNullOrEmpty(input.DiscountCode) ? 0 : Discounts.FirstOrDefault(d => d.Code == input.DiscountCode)?.Percent
            ?? throw new ValidationException("Mã giảm giá không hợp lệ.");
        var catalog = products.GetProducts().ToDictionary(p => p.Id);
        var items = input.Items.Select(item =>
        {
            if (!catalog.TryGetValue(item.ProductId, out var product) || !product.IsAvailable)
                throw new ValidationException("Một món không tồn tại hoặc đã ngừng bán. Vui lòng làm mới thực đơn.");
            return new OrderItem(product.Id, product.Name, product.Price, item.Quantity);
        }).ToArray();
        var subtotal = items.Sum(item => item.LineTotal);
        var discountAmount = decimal.Round(subtotal * discount / 100, 0, MidpointRounding.AwayFromZero);
        var tax = decimal.Round((subtotal - discountAmount) * input.TaxPercent / 100, 0, MidpointRounding.AwayFromZero);
        return new Order
        {
            RequestId = input.RequestId, OrderType = input.OrderType, PaymentMethod = input.PaymentMethod,
            DiscountPercent = discount, TaxPercent = input.TaxPercent, Note = input.Note?.Trim() ?? "",
            CreatedAt = clock.GetUtcNow(), Items = Array.AsReadOnly(items), Subtotal = subtotal,
            DiscountAmount = discountAmount, TaxAmount = tax, Total = subtotal - discountAmount + tax
        };
    }
}
