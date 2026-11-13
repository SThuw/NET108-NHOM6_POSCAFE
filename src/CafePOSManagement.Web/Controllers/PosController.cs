using System.ComponentModel.DataAnnotations;
using CafePOSManagement.Web.Models.ViewModels;
using CafePOSManagement.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CafePOSManagement.Web.Controllers;

[Authorize(Policy = "EmployeeOrManager")]
public sealed class PosController(IPosService posService, IProductService products, IOrderService orders) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View(posService.GetOverview());
    }

    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult Products(string? categoryId, string? search)
    {
        if (search?.Length > 100 || categoryId?.Length > 40)
            return BadRequest(new { message = "Từ khóa hoặc danh mục quá dài." });
        return Json(products.GetProducts(categoryId, search));
    }

    [HttpPost, Authorize(Policy = "ManagerOnly"), RequestSizeLimit(32768)]
    public IActionResult CreateProduct([FromBody] CreateProductDto input) => Execute(() => products.Create(input));

    [HttpPost, RequestSizeLimit(32768)]
    public IActionResult Preview([FromBody] CheckoutOrderDto input) => Execute(() => orders.Preview(input));

    [HttpPost, RequestSizeLimit(32768)]
    public IActionResult Checkout([FromBody] CheckoutOrderDto input) => Execute(() =>
        new { order = orders.Checkout(input), nextOrderNumber = orders.GetNextOrderNumber() });

    private IActionResult Execute<T>(Func<T> action)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Dữ liệu chưa hợp lệ. Kiểm tra các trường nhập.", errors = ModelState.Values.SelectMany(v => v.Errors)
                .Select(e => string.IsNullOrEmpty(e.ErrorMessage) || e.Exception is not null ? "Sai định dạng dữ liệu đầu vào." : e.ErrorMessage) });
        try { return Json(action()); }
        catch (ValidationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        Response.StatusCode = StatusCodes.Status500InternalServerError;
        return View("~/Views/Shared/Error.cshtml");
    }
}
