using System.ComponentModel.DataAnnotations;
using CafePOSManagement.Web.Data;
using CafePOSManagement.Web.Models.Entities;
using CafePOSManagement.Web.Models.ViewModels;
using CafePOSManagement.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafePOSManagement.Web.Controllers;

[Authorize(Policy = "ManagerOnly")]
public sealed class ProductsController(CafePosDbContext db, IProductService products) : Controller
{
    private const int PageSize = 10;

    [HttpGet]
    public async Task<IActionResult> Index(string? search, string? categoryId, string? status, int page = 1)
    {
        search = search?.Trim() ?? "";
        categoryId = categoryId?.Trim() ?? "";
        status = status is "active" or "stopped" ? status : "all";
        if (search.Length > 100 || categoryId.Length > 40) return BadRequest();
        var all = db.Products.AsNoTracking();
        var total = await all.CountAsync();
        var active = await all.CountAsync(p => p.IsAvailable);
        var query = all.Include(p => p.Category).AsQueryable();
        if (search.Length > 0) query = query.Where(p => p.Name.Contains(search) || p.Id.Contains(search));
        if (categoryId.Length > 0) query = query.Where(p => p.CategoryId == categoryId);
        if (status == "active") query = query.Where(p => p.IsAvailable);
        if (status == "stopped") query = query.Where(p => !p.IsAvailable);
        var resultCount = await query.CountAsync();
        var pageCount = Math.Max(1, (resultCount + PageSize - 1) / PageSize);
        page = Math.Clamp(page, 1, pageCount);
        return View(new ProductManagementViewModel
        {
            Products = await query.OrderBy(p => p.Name).ThenBy(p => p.Id).Skip((page - 1) * PageSize).Take(PageSize).ToListAsync(),
            Categories = await db.Categories.AsNoTracking()
                .Where(c => c.IsActive || db.Products.Any(p => p.CategoryId == c.Id))
                .OrderBy(c => c.DisplayOrder).ToListAsync(),
            Search = search, CategoryId = categoryId, Status = status, Page = page, PageCount = pageCount,
            Total = total, Active = active, Stopped = total - active, ResultCount = resultCount
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id, string? returnUrl)
    {
        var product = await db.Products.AsNoTracking().Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return NotFound();
        ViewBag.ReturnUrl = SafeReturnUrl(returnUrl);
        return View(product);
    }

    [HttpGet]
    public async Task<IActionResult> Create(string? returnUrl)
    {
        await PrepareForm(null, returnUrl);
        return View("Form", new CreateProductDto());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductDto input, string? returnUrl)
    {
        Normalize(input);
        if (await ValidateProduct(input, null))
        {
            try
            {
                products.Create(input);
                TempData["ProductNotice"] = "Đã thêm món mới.";
                return Redirect(SafeReturnUrl(returnUrl));
            }
            catch (ValidationException ex) { ModelState.AddModelError("", ex.Message); }
            catch (DbUpdateException) { ModelState.AddModelError("", "Không thể lưu món. Kiểm tra tên trùng hoặc thử lại."); }
        }
        await PrepareForm(null, returnUrl);
        return View("Form", input);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id, string? returnUrl)
    {
        var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return NotFound();
        await PrepareForm(product, returnUrl);
        return View("Form", new CreateProductDto
        {
            Name = product.Name, CategoryId = product.CategoryId, Price = product.Price,
            OriginalPrice = product.OriginalPrice, Description = product.Description,
            IsAvailable = product.IsAvailable
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, CreateProductDto input, string? returnUrl)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return NotFound();
        Normalize(input);
        if (await ValidateProduct(input, id))
        {
            var image = string.IsNullOrWhiteSpace(input.ImageUrl) ? product.ImageUrl : input.ImageUrl.Trim();
            db.Entry(product).CurrentValues.SetValues(product with
            {
                Name = input.Name, CategoryId = input.CategoryId, Price = input.Price,
                OriginalPrice = input.OriginalPrice, ImageUrl = image,
                Description = input.Description?.Trim(), IsAvailable = input.IsAvailable
            });
            try
            {
                await db.SaveChangesAsync();
                TempData["ProductNotice"] = "Đã cập nhật món.";
                return Redirect(SafeReturnUrl(returnUrl));
            }
            catch (DbUpdateException) { ModelState.AddModelError("", "Không thể cập nhật món. Kiểm tra tên trùng hoặc thử lại."); }
        }
        await PrepareForm(product, returnUrl);
        return View("Form", input);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(string id, string? returnUrl)
    {
        var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return NotFound();
        ViewBag.HasOrders = await db.OrderItems.AnyAsync(i => i.ProductId == id);
        ViewBag.ReturnUrl = SafeReturnUrl(returnUrl);
        return View(product);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id, string? returnUrl)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return NotFound();
        var hasOrders = await db.OrderItems.AnyAsync(i => i.ProductId == id);
        if (hasOrders) db.Entry(product).CurrentValues.SetValues(product with { IsAvailable = false });
        else db.Products.Remove(product);
        try
        {
            await db.SaveChangesAsync();
            TempData["ProductNotice"] = hasOrders ? "Món đã ngừng bán; hóa đơn cũ được giữ nguyên." : "Đã xóa món.";
            return Redirect(SafeReturnUrl(returnUrl));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "Không thể xóa món vì còn dữ liệu liên quan.");
            ViewBag.HasOrders = hasOrders;
            ViewBag.ReturnUrl = SafeReturnUrl(returnUrl);
            return View("Delete", product);
        }
    }

    private async Task<bool> ValidateProduct(CreateProductDto input, string? currentId)
    {
        if (!await db.Categories.AnyAsync(c => c.Id == input.CategoryId && c.IsActive))
            ModelState.AddModelError(nameof(input.CategoryId), "Chọn một danh mục đang hoạt động.");
        if (!string.IsNullOrWhiteSpace(input.Name) && await db.Products.AnyAsync(p => p.Id != currentId && p.CategoryId == input.CategoryId && p.Name == input.Name))
            ModelState.AddModelError(nameof(input.Name), "Tên món đã có trong danh mục này.");
        return ModelState.IsValid;
    }

    private async Task PrepareForm(Product? product, string? returnUrl)
    {
        var currentCategoryId = product?.CategoryId;
        ViewBag.Product = product;
        ViewBag.ReturnUrl = SafeReturnUrl(returnUrl);
        ViewBag.Categories = await db.Categories.AsNoTracking().Where(c => c.IsActive || c.Id == currentCategoryId)
            .OrderBy(c => c.DisplayOrder).ToListAsync();
    }

    private static void Normalize(CreateProductDto input)
    {
        input.Name = input.Name?.Trim() ?? "";
        input.CategoryId = input.CategoryId?.Trim() ?? "";
        input.ImageUrl = input.ImageUrl?.Trim();
    }

    private string SafeReturnUrl(string? returnUrl)
        => Url.IsLocalUrl(returnUrl) && returnUrl!.StartsWith("/Products", StringComparison.OrdinalIgnoreCase)
            ? returnUrl : Url.Action(nameof(Index), "Products")!;
}
