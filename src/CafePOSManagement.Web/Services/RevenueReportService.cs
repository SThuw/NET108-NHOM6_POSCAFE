using CafePOSManagement.Web.Data;
using CafePOSManagement.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CafePOSManagement.Web.Services;

/// <summary>
/// Tổng hợp báo cáo từ hóa đơn đã hoàn thành theo ngày lịch Việt Nam (UTC+7).
/// </summary>
public sealed class RevenueReportService(CafePosDbContext db)
{
    private static readonly TimeSpan VietnamOffset = TimeSpan.FromHours(7);
    private static readonly string[] ChartColors =
        ["#4E342E", "#4F772D", "#D99B52", "#A16236", "#8B6F63", "#78909C"];

    public async Task<RevenueReportViewModel> GetReportAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        var start = new DateTimeOffset(fromDate.Date, VietnamOffset);
        var endExclusive = new DateTimeOffset(toDate.Date.AddDays(1), VietnamOffset);
        var numberOfDays = (toDate.Date - fromDate.Date).Days + 1;
        var previousStart = start.AddDays(-numberOfDays);

        var currentOrders = await db.Orders
            .AsNoTracking()
            .AsSplitQuery()
            .Include(order => order.Items)
                .ThenInclude(item => item.Product)
                    .ThenInclude(product => product!.Category)
            .Include(order => order.CreatedByUser)
            .Where(order => order.Status == "Hoàn thành"
                && order.CreatedAt >= start
                && order.CreatedAt < endExclusive)
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync(cancellationToken);

        var previousOrders = await db.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .Where(order => order.Status == "Hoàn thành"
                && order.CreatedAt >= previousStart
                && order.CreatedAt < start)
            .ToListAsync(cancellationToken);

        var monthlyComparison = await BuildMonthlyComparisonAsync(toDate, cancellationToken);
        var totalRevenue = currentOrders.Sum(order => order.Total);
        var orderCount = currentOrders.Count;
        var totalCups = currentOrders.SelectMany(order => order.Items).Sum(item => item.Quantity);
        var averageRevenue = orderCount == 0 ? 0 : totalRevenue / orderCount;

        var previousRevenue = previousOrders.Sum(order => order.Total);
        var previousOrderCount = previousOrders.Count;
        var previousCups = previousOrders.SelectMany(order => order.Items).Sum(item => item.Quantity);
        var previousAverage = previousOrderCount == 0 ? 0 : previousRevenue / previousOrderCount;

        var trend = BuildDailyTrend(currentOrders, previousOrders, fromDate.Date, numberOfDays);
        var categories = BuildCategories(currentOrders);
        var channels = BuildChannels(currentOrders, totalRevenue);
        var payments = BuildPayments(currentOrders, totalRevenue);
        var peakHours = BuildPeakHours(currentOrders);
        var topProducts = BuildTopProducts(currentOrders);
        var recentOrders = BuildRecentOrders(currentOrders);

        return new RevenueReportViewModel
        {
            FromDate = fromDate.ToString("yyyy-MM-dd"),
            ToDate = toDate.ToString("yyyy-MM-dd"),
            PeriodRangeText = $"Từ {fromDate:dd/MM/yyyy} đến {toDate:dd/MM/yyyy}",
            OrderCount = orderCount,
            TotalRevenue = totalRevenue,
            AverageRevenuePerOrder = averageRevenue,
            TotalCups = totalCups,
            Kpis = new RevenueKpiSetViewModel
            {
                Revenue = Metric(totalRevenue, previousRevenue,
                    $"So với kỳ trước: {previousRevenue:N0} ₫", trend.CurrentRevenue.TakeLast(8)),
                Orders = Metric(orderCount, previousOrderCount,
                    $"Kỳ trước có {previousOrderCount:N0} đơn", trend.Orders.TakeLast(8).Select(value => (decimal)value)),
                Aov = Metric(averageRevenue, previousAverage,
                    "Giá trị trung bình mỗi hóa đơn", trend.AverageOrderValue.TakeLast(8)),
                Cups = Metric(totalCups, previousCups,
                    $"Kỳ trước bán {previousCups:N0} ly", trend.Cups.TakeLast(8).Select(value => (decimal)value))
            },
            Trend = trend,
            Categories = categories,
            Channels = channels,
            Payments = payments,
            MonthlyComparison = monthlyComparison,
            PeakHours = peakHours,
            TopProducts = topProducts,
            RecentOrders = recentOrders
        };
    }

    private async Task<RevenueMonthlyComparisonViewModel> BuildMonthlyComparisonAsync(
        DateTime toDate,
        CancellationToken cancellationToken)
    {
        var endMonth = new DateTime(toDate.Year, toDate.Month, 1);
        var firstActualMonth = endMonth.AddMonths(-11);
        var queryStart = new DateTimeOffset(firstActualMonth.AddYears(-1), VietnamOffset);
        var queryEnd = new DateTimeOffset(endMonth.AddMonths(1), VietnamOffset);

        var rows = await db.Orders
            .AsNoTracking()
            .Where(order => order.Status == "Hoàn thành"
                && order.CreatedAt >= queryStart
                && order.CreatedAt < queryEnd)
            .Select(order => new MonthlyOrderRow(order.CreatedAt, order.Total))
            .ToListAsync(cancellationToken);

        var totals = rows
            .GroupBy(row =>
            {
                var localDate = row.CreatedAt.ToOffset(VietnamOffset);
                return (localDate.Year, localDate.Month);
            })
            .ToDictionary(group => group.Key, group => group.Sum(row => row.Total));

        RevenueMonthlyRangeViewModel BuildRange(int count)
        {
            var months = Enumerable.Range(0, count)
                .Select(index => endMonth.AddMonths(index - count + 1))
                .ToArray();

            return new RevenueMonthlyRangeViewModel
            {
                Labels = months.Select(month => $"T{month:MM}/{month:yy}").ToArray(),
                Actual = months.Select(month => totals.GetValueOrDefault((month.Year, month.Month))).ToArray(),
                Previous = months.Select(month => totals.GetValueOrDefault((month.Year - 1, month.Month))).ToArray()
            };
        }

        return new RevenueMonthlyComparisonViewModel
        {
            Months6 = BuildRange(6),
            Months12 = BuildRange(12)
        };
    }

    private static RevenueTrendViewModel BuildDailyTrend(
        IReadOnlyList<Order> currentOrders,
        IReadOnlyList<Order> previousOrders,
        DateTime fromDate,
        int numberOfDays)
    {
        var currentByDay = currentOrders
            .GroupBy(order => order.CreatedAt.ToOffset(VietnamOffset).Date)
            .ToDictionary(group => group.Key, group => new DailyTotal(
                group.Sum(order => order.Total),
                group.Count(),
                group.SelectMany(order => order.Items).Sum(item => item.Quantity)));

        var previousByDay = previousOrders
            .GroupBy(order => order.CreatedAt.ToOffset(VietnamOffset).Date)
            .ToDictionary(group => group.Key, group => group.Sum(order => order.Total));

        var previousFromDate = fromDate.AddDays(-numberOfDays);
        var labels = new List<string>(numberOfDays);
        var currentRevenue = new List<decimal>(numberOfDays);
        var previousRevenue = new List<decimal>(numberOfDays);
        var orders = new List<int>(numberOfDays);
        var averageOrderValue = new List<decimal>(numberOfDays);
        var cups = new List<int>(numberOfDays);

        for (var index = 0; index < numberOfDays; index++)
        {
            var date = fromDate.AddDays(index);
            var previousDate = previousFromDate.AddDays(index);
            var total = currentByDay.GetValueOrDefault(date, DailyTotal.Empty);

            labels.Add(date.ToString("dd/MM"));
            currentRevenue.Add(total.Revenue);
            previousRevenue.Add(previousByDay.GetValueOrDefault(previousDate));
            orders.Add(total.OrderCount);
            averageOrderValue.Add(total.OrderCount == 0 ? 0 : total.Revenue / total.OrderCount);
            cups.Add(total.Cups);
        }

        return new RevenueTrendViewModel
        {
            Labels = labels,
            CurrentRevenue = currentRevenue,
            PreviousRevenue = previousRevenue,
            Orders = orders,
            AverageOrderValue = averageOrderValue,
            Cups = cups
        };
    }

    private static IReadOnlyList<RevenueCategoryViewModel> BuildCategories(IReadOnlyList<Order> orders)
    {
        return orders
            .SelectMany(order => order.Items)
            .GroupBy(item => item.Product?.Category?.Name ?? "Chưa phân loại")
            .Select((group, index) => new RevenueCategoryViewModel
            {
                Name = group.Key,
                Value = group.Sum(item => item.LineTotal),
                Color = ChartColors[index % ChartColors.Length]
            })
            .OrderByDescending(item => item.Value)
            .ToArray();
    }

    private static RevenueChannelsViewModel BuildChannels(IReadOnlyList<Order> orders, decimal totalRevenue)
    {
        if (totalRevenue <= 0) return new RevenueChannelsViewModel();
        var posRevenue = orders.Where(order => !string.IsNullOrWhiteSpace(order.CreatedByUserId)).Sum(order => order.Total);
        var posPercent = Percentage(posRevenue, totalRevenue);
        return new RevenueChannelsViewModel { PosPct = posPercent, KioskPct = 100 - posPercent };
    }

    private static RevenuePaymentsViewModel BuildPayments(IReadOnlyList<Order> orders, decimal totalRevenue)
    {
        if (totalRevenue <= 0) return new RevenuePaymentsViewModel();
        var cash = orders.Where(order => order.PaymentMethod == "cash").Sum(order => order.Total);
        var card = orders.Where(order => order.PaymentMethod == "card").Sum(order => order.Total);
        var cashPercent = Percentage(cash, totalRevenue);
        var cardPercent = Percentage(card, totalRevenue);
        return new RevenuePaymentsViewModel
        {
            CashPct = cashPercent,
            CardPct = cardPercent,
            WalletPct = Math.Max(0, 100 - cashPercent - cardPercent)
        };
    }

    private static RevenuePeakHoursViewModel BuildPeakHours(IReadOnlyList<Order> orders)
    {
        var byHour = orders
            .GroupBy(order => order.CreatedAt.ToOffset(VietnamOffset).Hour)
            .ToDictionary(group => group.Key, group => new
            {
                Revenue = group.Sum(order => order.Total),
                Count = group.Count()
            });

        var hours = Enumerable.Range(7, 16).ToArray();
        return new RevenuePeakHoursViewModel
        {
            Labels = hours.Select(hour => $"{hour:00}:00").ToArray(),
            Revenue = hours.Select(hour => byHour.TryGetValue(hour, out var value) ? value.Revenue : 0).ToArray(),
            Orders = hours.Select(hour => byHour.TryGetValue(hour, out var value) ? value.Count : 0).ToArray()
        };
    }

    private static IReadOnlyList<RevenueTopProductViewModel> BuildTopProducts(IReadOnlyList<Order> orders)
    {
        var ranked = orders
            .SelectMany(order => order.Items)
            .GroupBy(item => new
            {
                item.ProductId,
                item.ProductName,
                Category = item.Product?.Category?.Name ?? "Chưa phân loại",
                Image = item.Product?.ImageUrl ?? "/images/product-fallback.svg"
            })
            .Select(group => new
            {
                group.Key.ProductName,
                group.Key.Category,
                group.Key.Image,
                Cups = group.Sum(item => item.Quantity),
                Revenue = group.Sum(item => item.LineTotal)
            })
            .OrderByDescending(item => item.Cups)
            .ThenByDescending(item => item.Revenue)
            .ThenBy(item => item.ProductName)
            .Take(5)
            .ToArray();

        var topQuantity = ranked.FirstOrDefault()?.Cups ?? 0;
        return ranked.Select((item, index) => new RevenueTopProductViewModel
        {
            Rank = index + 1,
            Name = item.ProductName,
            Category = item.Category,
            Image = item.Image,
            Cups = item.Cups,
            Revenue = item.Revenue,
            Share = topQuantity == 0 ? 0 : (int)Math.Round(item.Cups * 100m / topQuantity)
        }).ToArray();
    }

    private static IReadOnlyList<RevenueRecentOrderViewModel> BuildRecentOrders(IReadOnlyList<Order> orders)
    {
        return orders
            .OrderByDescending(order => order.CreatedAt)
            .Take(6)
            .Select(order => new RevenueRecentOrderViewModel
            {
                Code = order.OrderNumber,
                Time = order.CreatedAt.ToOffset(VietnamOffset).ToString("dd/MM/yyyy HH:mm"),
                Channel = string.IsNullOrWhiteSpace(order.CreatedByUserId) ? "Kiosk/Khác" : "POS",
                Payment = PaymentLabel(order.PaymentMethod),
                Cashier = order.CreatedByUser?.FullName ?? "Không xác định",
                Subtotal = order.Subtotal,
                Discount = order.DiscountAmount,
                Total = order.Total,
                Note = order.Note,
                Items = order.Items.Select(item => new RevenueOrderItemViewModel
                {
                    Name = item.ProductName,
                    Qty = item.Quantity,
                    UnitPrice = item.UnitPrice
                }).ToArray()
            })
            .ToArray();
    }

    private static RevenueMetricViewModel Metric(
        decimal value,
        decimal previousValue,
        string sub,
        IEnumerable<decimal> sparkline)
    {
        var points = sparkline.ToArray();
        return new RevenueMetricViewModel
        {
            Value = value,
            Growth = Growth(value, previousValue),
            Sub = sub,
            Sparkline = points.Length == 0 ? [0] : points
        };
    }

    private static double Growth(decimal current, decimal previous)
    {
        if (previous == 0) return current == 0 ? 0 : 100;
        return Math.Round((double)((current - previous) / previous * 100), 1);
    }

    private static int Percentage(decimal part, decimal total)
        => total <= 0 ? 0 : (int)Math.Round(part * 100m / total);

    private static string PaymentLabel(string paymentMethod) => paymentMethod switch
    {
        "cash" => "Tiền mặt",
        "card" => "Thẻ ngân hàng",
        "wallet" => "Ví điện tử",
        _ => paymentMethod
    };

    private sealed record MonthlyOrderRow(DateTimeOffset CreatedAt, decimal Total);
    private sealed record DailyTotal(decimal Revenue, int OrderCount, int Cups)
    {
        public static DailyTotal Empty { get; } = new(0, 0, 0);
    }
}

public sealed record RevenueReportViewModel
{
    public bool Invalid { get; init; }
    public string Message { get; init; } = "";
    public string FromDate { get; init; } = "";
    public string ToDate { get; init; } = "";
    public string CurrentDate { get; init; } = "";
    public string ActivePeriod { get; init; } = "custom";
    public string PeriodRangeText { get; init; } = "";
    public int OrderCount { get; init; }
    public decimal TotalRevenue { get; init; }
    public decimal AverageRevenuePerOrder { get; init; }
    public int TotalCups { get; init; }
    public RevenueKpiSetViewModel Kpis { get; init; } = new();
    public RevenueTrendViewModel Trend { get; init; } = new();
    public IReadOnlyList<RevenueCategoryViewModel> Categories { get; init; } = [];
    public RevenueChannelsViewModel Channels { get; init; } = new();
    public RevenuePaymentsViewModel Payments { get; init; } = new();
    public RevenueMonthlyComparisonViewModel MonthlyComparison { get; init; } = new();
    public RevenuePeakHoursViewModel PeakHours { get; init; } = new();
    public IReadOnlyList<RevenueTopProductViewModel> TopProducts { get; init; } = [];
    public IReadOnlyList<RevenueRecentOrderViewModel> RecentOrders { get; init; } = [];

    public static RevenueReportViewModel InvalidRange(
        DateTime fromDate,
        DateTime toDate,
        DateTime currentDate,
        string message) => new()
        {
            Invalid = true,
            Message = message,
            FromDate = fromDate.ToString("yyyy-MM-dd"),
            ToDate = toDate.ToString("yyyy-MM-dd"),
            CurrentDate = currentDate.ToString("yyyy-MM-dd"),
            PeriodRangeText = "Khoảng thời gian chưa hợp lệ"
        };
}

public sealed record RevenueKpiSetViewModel
{
    public RevenueMetricViewModel Revenue { get; init; } = new();
    public RevenueMetricViewModel Orders { get; init; } = new();
    public RevenueMetricViewModel Aov { get; init; } = new();
    public RevenueMetricViewModel Cups { get; init; } = new();
}

public sealed record RevenueMetricViewModel
{
    public decimal Value { get; init; }
    public double Growth { get; init; }
    public string Sub { get; init; } = "";
    public IReadOnlyList<decimal> Sparkline { get; init; } = [];
}

public sealed record RevenueTrendViewModel
{
    public IReadOnlyList<string> Labels { get; init; } = [];
    public IReadOnlyList<decimal> CurrentRevenue { get; init; } = [];
    public IReadOnlyList<decimal> PreviousRevenue { get; init; } = [];
    public IReadOnlyList<int> Orders { get; init; } = [];
    public IReadOnlyList<decimal> AverageOrderValue { get; init; } = [];
    public IReadOnlyList<int> Cups { get; init; } = [];
}

public sealed record RevenueCategoryViewModel
{
    public string Name { get; init; } = "";
    public decimal Value { get; init; }
    public string Color { get; init; } = "#4E342E";
}

public sealed record RevenueChannelsViewModel
{
    public int PosPct { get; init; }
    public int KioskPct { get; init; }
}

public sealed record RevenuePaymentsViewModel
{
    public int CashPct { get; init; }
    public int CardPct { get; init; }
    public int WalletPct { get; init; }
}

public sealed record RevenueMonthlyComparisonViewModel
{
    public RevenueMonthlyRangeViewModel Months6 { get; init; } = new();
    public RevenueMonthlyRangeViewModel Months12 { get; init; } = new();
}

public sealed record RevenueMonthlyRangeViewModel
{
    public IReadOnlyList<string> Labels { get; init; } = [];
    public IReadOnlyList<decimal> Actual { get; init; } = [];
    public IReadOnlyList<decimal> Previous { get; init; } = [];
}

public sealed record RevenuePeakHoursViewModel
{
    public IReadOnlyList<string> Labels { get; init; } = [];
    public IReadOnlyList<decimal> Revenue { get; init; } = [];
    public IReadOnlyList<int> Orders { get; init; } = [];
}

public sealed record RevenueTopProductViewModel
{
    public int Rank { get; init; }
    public string Name { get; init; } = "";
    public string Category { get; init; } = "";
    public string Image { get; init; } = "";
    public int Cups { get; init; }
    public decimal Revenue { get; init; }
    public int Share { get; init; }
}

public sealed record RevenueRecentOrderViewModel
{
    public string Code { get; init; } = "";
    public string Time { get; init; } = "";
    public string Channel { get; init; } = "";
    public string Payment { get; init; } = "";
    public string Cashier { get; init; } = "";
    public decimal Subtotal { get; init; }
    public decimal Discount { get; init; }
    public decimal Total { get; init; }
    public string Note { get; init; } = "";
    public IReadOnlyList<RevenueOrderItemViewModel> Items { get; init; } = [];
}

public sealed record RevenueOrderItemViewModel
{
    public string Name { get; init; } = "";
    public int Qty { get; init; }
    public decimal UnitPrice { get; init; }
}
