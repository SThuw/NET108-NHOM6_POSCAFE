using System.ComponentModel.DataAnnotations;
using CafePOSManagement.Web.Data;
using CafePOSManagement.Web.Models.Entities;
using CafePOSManagement.Web.Models.ViewModels;
using CafePOSManagement.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CafePOSManagement.Web.Services.Implementations;

/// <summary>
/// Triển khai IOrderService dùng EF Core / SQL Server (Giai đoạn 4+).
/// - GetDiscountCodes / GetTaxRates đọc từ DB.
/// - Preview: tính tạm tính, không lưu.
/// - Checkout: dùng transaction + unique index trên RequestId để chống gửi trùng.
/// </summary>
public sealed class DbOrderService(CafePosDbContext db, IProductService products, TimeProvider clock) : IOrderService
{
    private static readonly decimal[] TaxRates = [0, 5, 8, 10];

    public IReadOnlyList<DiscountCode> GetDiscountCodes()
        => db.Discounts
             .Where(d => d.IsActive)
             .OrderBy(d => d.Id)
             .AsNoTracking()
             .ToArray()
             .Select(d => new DiscountCode(d.Code, d.Name, d.DiscountPercent))
             .ToArray();

    public IReadOnlyList<decimal> GetTaxRates() => TaxRates;

    public string GetNextOrderNumber()
    {
        // Đọc số thứ tự lớn nhất hiện tại từ DB và cộng thêm 1 để ước tính.
        // Số chính thức được cấp trong Checkout (transaction). Đây chỉ là hiển thị dự kiến.
        var last = db.Orders
                     .OrderByDescending(o => o.Id)
                     .Select(o => o.OrderNumber)
                     .FirstOrDefault() ?? "#000";
        if (int.TryParse(last.TrimStart('#'), out var num))
            return $"#{num + 1:000}";
        return "#004";
    }

    public Order Preview(CheckoutOrderDto input)
    {
        Validate(input);
        return Calculate(input) with { OrderNumber = GetNextOrderNumber() };
    }

    public Order Checkout(CheckoutOrderDto input)
    {
        Validate(input);

        // Kiểm tra idempotency trước khi mở transaction.
        var existing = db.Orders
            .Include(o => o.Items)
            .AsNoTracking()
            .FirstOrDefault(o => o.RequestId == input.RequestId);
        if (existing is not null)
            return existing;

        var order = Calculate(input);

        // Lấy DiscountId nếu có mã giảm giá.
        int? discountId = null;
        if (!string.IsNullOrWhiteSpace(input.DiscountCode))
        {
            discountId = db.Discounts
                .Where(d => d.Code == input.DiscountCode && d.IsActive)
                .Select(d => (int?)d.Id)
                .FirstOrDefault()
                ?? throw new ValidationException("Mã giảm giá không hợp lệ.");
        }

        // Tạo số thứ tự đơn trong transaction; sử dụng sequence DB-side để tránh race condition.
        using var transaction = db.Database.BeginTransaction(
            System.Data.IsolationLevel.ReadCommitted);
        try
        {
            var lastNum = db.Orders
                .OrderByDescending(o => o.Id)
                .Select(o => o.OrderNumber)
                .FirstOrDefault() ?? "#000";
            var seq = int.TryParse(lastNum.TrimStart('#'), out var n) ? n + 1 : 4;
            var orderNumber = $"#{seq:000}";

            var entity = order with
            {
                OrderNumber = orderNumber,
                DiscountId = discountId,
                CreatedAt = clock.GetUtcNow()
            };

            db.Orders.Add(entity);
            db.SaveChanges();
            transaction.Commit();
            return entity;
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            // RequestId đã được lưu bởi request song song → trả đơn đó.
            transaction.Rollback();
            return db.Orders
                .Include(o => o.Items)
                .AsNoTracking()
                .First(o => o.RequestId == input.RequestId);
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    // ── Nội bộ ──────────────────────────────────────────────────────────────

    private void Validate(CheckoutOrderDto input)
    {
        Validator.ValidateObject(input, new ValidationContext(input), true);
        foreach (var item in input.Items)
            Validator.ValidateObject(item, new ValidationContext(item), true);
        if (input.Items.Select(i => i.ProductId).Distinct().Count() != input.Items.Count)
            throw new ValidationException("Mỗi món chỉ được xuất hiện một dòng trong đơn.");
        if (!TaxRates.Contains(input.TaxPercent))
            throw new ValidationException("Mức VAT chưa được cấu hình.");
    }

    private Order Calculate(CheckoutOrderDto input)
    {
        var discountPercent = 0m;
        if (!string.IsNullOrWhiteSpace(input.DiscountCode))
        {
            var discount = db.Discounts
                .AsNoTracking()
                .FirstOrDefault(d => d.Code == input.DiscountCode && d.IsActive)
                ?? throw new ValidationException("Mã giảm giá không hợp lệ.");
            discountPercent = discount.DiscountPercent;
        }

        var catalog = products.GetProducts().ToDictionary(p => p.Id);
        var items = input.Items.Select(i =>
        {
            if (!catalog.TryGetValue(i.ProductId, out var p) || !p.IsAvailable)
                throw new ValidationException("Một món không tồn tại hoặc đã ngừng bán. Vui lòng làm mới thực đơn.");
            return new OrderItem(p.Id, p.Name, p.Price, i.Quantity);
        }).ToArray();

        var subtotal = items.Sum(i => i.LineTotal);
        var discountAmount = decimal.Round(subtotal * discountPercent / 100, 0, MidpointRounding.AwayFromZero);
        var tax = decimal.Round((subtotal - discountAmount) * input.TaxPercent / 100, 0, MidpointRounding.AwayFromZero);

        return new Order
        {
            RequestId = input.RequestId,
            OrderType = input.OrderType,
            PaymentMethod = input.PaymentMethod,
            DiscountPercent = discountPercent,
            TaxPercent = input.TaxPercent,
            Note = input.Note?.Trim() ?? "",
            Status = "Hoàn thành",
            CreatedAt = clock.GetUtcNow(),
            Items = Array.AsReadOnly(items),
            Subtotal = subtotal,
            DiscountAmount = discountAmount,
            TaxAmount = tax,
            Total = subtotal - discountAmount + tax
        };
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        // SQL Server error 2601 (unique index) hoặc 2627 (unique constraint).
        var inner = ex.InnerException?.Message ?? "";
        return inner.Contains("2601") || inner.Contains("2627")
               || inner.Contains("UNIQUE") || inner.Contains("IX_Orders_RequestId");
    }
}
