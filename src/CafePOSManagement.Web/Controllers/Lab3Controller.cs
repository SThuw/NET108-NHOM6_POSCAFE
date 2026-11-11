using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using CafePOSManagement.Web.DAL;

namespace CafePOSManagement.Web.Controllers;

public class Lab3ViewModel
{
    public List<CategoryDAL> Categories { get; set; } = new();
    public List<ProductDAL> Products { get; set; } = new();
    public List<UserDAL> Users { get; set; } = new();
    public List<CardDAL> Cards { get; set; } = new();
    public List<OrderDAL> Orders { get; set; } = new();
}

public class Lab3Controller : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Title"] = "Báo cáo Thực hành LAB 3 - Mô hình 3 Lớp & ADO.NET";
        var model = new Lab3ViewModel
        {
            Categories = CategoryDAL.GetAll(),
            Products = ProductDAL.GetAll(),
            Users = UserDAL.GetAll(),
            Cards = CardDAL.GetAll(),
            Orders = OrderDAL.GetAll()
        };
        return View(model);
    }
}
