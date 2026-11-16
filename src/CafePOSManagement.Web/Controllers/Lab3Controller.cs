using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using CafePOSManagement.Web.DAL;
using Microsoft.AspNetCore.Authorization;

namespace CafePOSManagement.Web.Controllers;

public class Lab3ViewModel
{
    public List<CategoryDAL> Categories { get; set; } = new();
    public List<ProductDAL> Products { get; set; } = new();
    public List<UserDAL> Users { get; set; } = new();
    public List<CardDAL> Cards { get; set; } = new();
    public List<OrderDAL> Orders { get; set; } = new();
    public List<AiSessionDTO> AiSessions { get; set; } = new();
}

[Authorize(Policy = "ManagerOnly")]
public class Lab3Controller : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Title"] = "Quản lý Hệ thống · PolyCafe";
        var model = new Lab3ViewModel();
        try
        {
            model.Categories = CategoryDAL.GetAll();
            model.Products = ProductDAL.GetAll();
            model.Users = UserDAL.GetAll();
            model.Cards = CardDAL.GetAll();
            model.Orders = OrderDAL.GetAll();
            model.AiSessions = AiStateStoreDAL.GetAllSessions();
        }
        catch (Exception ex)
        {
            ViewBag.DbError = ex.Message;
        }
        return View(model);
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
public IActionResult UpdateOrderStatus(long id, string newStatus)
{
    try
    {
        // Lấy đơn hàng hiện tại từ DB
        var order = OrderDAL.GetById(id);
        if (order == null)
        {
            return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
        }

        // Cập nhật trạng thái mới
        order.Status = newStatus;
        
        // Gọi hàm Update có sẵn trong DAL (cập nhật Status và Note)
        OrderDAL.Update(order);

        return Json(new { success = true, message = "Cập nhật trạng thái thành công!" });
    }
    catch (Exception ex)
    {
        return Json(new { success = false, message = "Lỗi: " + ex.Message });
    }
}
}
