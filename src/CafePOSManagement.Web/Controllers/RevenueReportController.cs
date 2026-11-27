using CafePOSManagement.Web.Data;
using CafePOSManagement.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafePOSManagement.Web.Controllers;

[Authorize(Policy = "ManagerOnly")]
public sealed class RevenueReportController(CafePosDbContext db) : Controller
{
    private readonly RevenueReportService _service = new(db);

    [HttpGet]
    public async Task<IActionResult> Index(
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken)
    {
        if (fromDate.HasValue && toDate.HasValue && fromDate.Value.Date > toDate.Value.Date)
        {
            ModelState.AddModelError(string.Empty, "Ngày bắt đầu không được lớn hơn ngày kết thúc.");
            return View(new RevenueReportViewModel
            {
                FromDate = fromDate,
                ToDate = toDate
            });
        }

        var model = await _service.GetReportAsync(fromDate, toDate, cancellationToken);
        return View(model);
    }
}
