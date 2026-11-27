using CafePOSManagement.Web.Data;
using CafePOSManagement.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafePOSManagement.Web.Controllers;

[Authorize(Policy = "ManagerOnly")]
public sealed class TopSellingController(CafePosDbContext db) : Controller
{
    private readonly TopSellingService _service = new(db);

    [HttpGet]
    public async Task<IActionResult> Index(
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken)
    {
        if (fromDate.HasValue && toDate.HasValue && fromDate.Value.Date > toDate.Value.Date)
        {
            ModelState.AddModelError(string.Empty, "Ngày bắt đầu không được lớn hơn ngày kết thúc.");
            var emptyModel = await _service.GetTopSellingAsync(null, null, cancellationToken);
            emptyModel = emptyModel with { FromDate = fromDate, ToDate = toDate };
            return View(emptyModel);
        }

        var model = await _service.GetTopSellingAsync(fromDate, toDate, cancellationToken);
        return View(model);
    }
}
