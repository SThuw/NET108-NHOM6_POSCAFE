using CafePOSManagement.Web.Data;
using CafePOSManagement.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafePOSManagement.Web.Controllers;

[Authorize(Policy = "ManagerOnly")]
public sealed class RevenueReportController(CafePosDbContext db, TimeProvider clock) : Controller
{
    private static readonly TimeSpan VietnamOffset = TimeSpan.FromHours(7);
    private readonly RevenueReportService _service = new(db);

    [HttpGet]
    public async Task<IActionResult> Index(
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var today = clock.GetUtcNow().ToOffset(VietnamOffset).Date;
        var resolvedTo = toDate?.Date ?? today;
        var resolvedFrom = fromDate?.Date ?? new DateTime(resolvedTo.Year, resolvedTo.Month, 1);

        if (resolvedFrom > resolvedTo)
        {
            ModelState.AddModelError(string.Empty, "Ngày bắt đầu không được lớn hơn ngày kết thúc.");
            return View(RevenueReportViewModel.InvalidRange(
                resolvedFrom,
                resolvedTo,
                today,
                "Ngày bắt đầu không được lớn hơn ngày kết thúc. Vui lòng chọn lại khoảng thời gian hợp lệ."));
        }

        if ((resolvedTo - resolvedFrom).TotalDays > 366)
        {
            ModelState.AddModelError(string.Empty, "Khoảng thời gian thống kê tối đa là 367 ngày.");
            return View(RevenueReportViewModel.InvalidRange(
                resolvedFrom,
                resolvedTo,
                today,
                "Khoảng thời gian quá dài. Vui lòng chọn tối đa 367 ngày để bảo đảm tốc độ báo cáo."));
        }

        var model = await _service.GetReportAsync(resolvedFrom, resolvedTo, cancellationToken);
        return View(model with
        {
            CurrentDate = today.ToString("yyyy-MM-dd"),
            ActivePeriod = ResolveActivePeriod(resolvedFrom, resolvedTo, today)
        });
    }

    private static string ResolveActivePeriod(DateTime fromDate, DateTime toDate, DateTime today)
    {
        if (fromDate == today && toDate == today) return "today";
        if (fromDate == today.AddDays(-6) && toDate == today) return "week";
        if (fromDate == new DateTime(today.Year, today.Month, 1) && toDate == today) return "month";
        if (fromDate == new DateTime(today.Year, 1, 1) && toDate == today) return "year";
        return "custom";
    }
}
