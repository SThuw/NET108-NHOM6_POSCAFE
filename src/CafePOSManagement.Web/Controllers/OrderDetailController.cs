using CafePOSManagement.Web.Data;
using CafePOSManagement.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafePOSManagement.Web.Controllers;

[Authorize(Policy = "EmployeeOrManager")]
public sealed class OrderDetailController(CafePosDbContext db) : Controller
{
    [HttpGet("/OrderDetail/{id:long}")]
    public async Task<IActionResult> Index(long id)
    {
        var order = await db.Orders
            .AsNoTracking()
            .Include(item => item.Items)
            .Include(item => item.CreatedByUser)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (order is null)
            return NotFound();

        return View(new OrderDetailViewModel(order, order.CreatedByUser?.FullName));
    }
}
