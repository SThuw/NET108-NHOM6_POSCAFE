using CafePOSManagement.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CafePOSManagement.Web.Services;

/// <summary>
/// Tổng hợp doanh thu từ các hóa đơn đã hoàn thành trong khoảng ngày Việt Nam.
/// </summary>
public sealed class RevenueReportService(CafePosDbContext db)
{
    private static readonly TimeSpan VietnamOffset = TimeSpan.FromHours(7);

    public async Task<RevenueReportViewModel> GetReportAsync(
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var start = fromDate.HasValue
            ? new DateTimeOffset(fromDate.Value.Date, VietnamOffset)
            : (DateTimeOffset?)null;

        // Mốc kết thúc không bao gồm ngày kế tiếp để lấy trọn ngày được chọn.
        var endExclusive = toDate.HasValue && toDate.Value.Date < DateTime.MaxValue.Date
            ? new DateTimeOffset(toDate.Value.Date.AddDays(1), VietnamOffset)
            : (DateTimeOffset?)null;

        var query = db.Orders
            .AsNoTracking()
            .Where(order => order.Status == "Hoàn thành");

        if (start.HasValue)
            query = query.Where(order => order.CreatedAt >= start.Value);

        if (endExclusive.HasValue)
            query = query.Where(order => order.CreatedAt < endExclusive.Value);

        var summary = await query
            .GroupBy(_ => 1)
            .Select(group => new
            {
                OrderCount = group.Count(),
                TotalRevenue = group.Sum(order => order.Total)
            })
            .SingleOrDefaultAsync(cancellationToken);

        var orderCount = summary?.OrderCount ?? 0;
        var totalRevenue = summary?.TotalRevenue ?? 0m;

        return new RevenueReportViewModel
        {
            FromDate = fromDate,
            ToDate = toDate,
            OrderCount = orderCount,
            TotalRevenue = totalRevenue,
            AverageRevenuePerOrder = orderCount == 0 ? 0 : totalRevenue / orderCount
        };
    }
}

public sealed record RevenueReportViewModel
{
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public int OrderCount { get; init; }
    public decimal TotalRevenue { get; init; }
    public decimal AverageRevenuePerOrder { get; init; }
}
