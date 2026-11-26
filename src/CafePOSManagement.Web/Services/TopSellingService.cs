using CafePOSManagement.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CafePOSManagement.Web.Services;

/// <summary>
/// Tổng hợp 5 sản phẩm bán chạy dựa trên các hóa đơn đã hoàn thành.
/// Doanh thu lấy từ LineTotal đã lưu tại thời điểm tạo hóa đơn,
/// không lấy giá hiện tại trong bảng Products.
/// </summary>
public sealed class TopSellingService(CafePosDbContext db)
{
    public async Task<TopSellingViewModel> GetTopSellingAsync(
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        // Dự án đặt Culture mặc định vi-VN. Dùng múi giờ UTC+7 để
        // khoảng ngày được hiểu theo ngày lịch Việt Nam.
        var start = fromDate.HasValue
            ? new DateTimeOffset(fromDate.Value.Date, TimeSpan.FromHours(7))
            : (DateTimeOffset?)null;

        // Dùng mốc kết thúc không bao gồm ngày kế tiếp để lấy trọn ngày toDate.
        var endExclusive = toDate.HasValue
            ? new DateTimeOffset(toDate.Value.Date.AddDays(1), TimeSpan.FromHours(7))
            : (DateTimeOffset?)null;

        var query = db.OrderItems
            .AsNoTracking()
            .Where(item => item.Order != null && item.Order.Status == "Hoàn thành");

        if (start.HasValue)
            query = query.Where(item => item.Order!.CreatedAt >= start.Value);

        if (endExclusive.HasValue)
            query = query.Where(item => item.Order!.CreatedAt < endExclusive.Value);

        var topItems = await query
            .GroupBy(item => new { item.ProductId, item.ProductName })
            .Select(group => new TopSellingItemViewModel
            {
                ProductId = group.Key.ProductId,
                ProductName = group.Key.ProductName,
                QuantitySold = group.Sum(item => item.Quantity),
                Revenue = group.Sum(item => item.LineTotal)
            })
            .OrderByDescending(item => item.QuantitySold)
            .ThenByDescending(item => item.Revenue)
            .ThenBy(item => item.ProductName)
            .Take(5)
            .ToListAsync(cancellationToken);

        return new TopSellingViewModel
        {
            FromDate = fromDate,
            ToDate = toDate,
            Items = topItems,
            TotalQuantity = topItems.Sum(item => item.QuantitySold),
            TotalRevenue = topItems.Sum(item => item.Revenue)
        };
    }
}

public sealed record TopSellingViewModel
{
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public IReadOnlyList<TopSellingItemViewModel> Items { get; init; } = [];
    public int TotalQuantity { get; init; }
    public decimal TotalRevenue { get; init; }
}

public sealed class TopSellingItemViewModel
{
    public string ProductId { get; init; } = "";
    public string ProductName { get; init; } = "";
    public int QuantitySold { get; init; }
    public decimal Revenue { get; init; }
}
