using CafePOSManagement.Web.Data;
using CafePOSManagement.Web.Models.Entities;
using CafePOSManagement.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafePOSManagement.Web.Controllers;

[Authorize(Policy = "ManagerOnly")]
public sealed class CategoriesController(CafePosDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index() => View(await db.Categories.AsNoTracking()
        .Where(c => c.IsActive).OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name).ToListAsync());

    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        var category = await db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
        return category is null ? NotFound() : View(category);
    }

    [HttpGet, Authorize(Policy = "ManagerOnly")]
    public IActionResult Create() => View("Form", new CategoryForm());

    [HttpPost, Authorize(Policy = "ManagerOnly"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryForm form)
    {
        Normalize(form);
        if (!ModelState.IsValid) return View("Form", form);
        if (await db.Categories.AnyAsync(c => c.Id == form.Id || c.Code == form.Code || c.Name == form.Name))
        {
            ModelState.AddModelError("", "Mã hoặc tên danh mục đã tồn tại.");
            return View("Form", form);
        }
        db.Categories.Add(new Category(form.Id, form.Name) { Code = form.Code, DisplayOrder = form.DisplayOrder });
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException) { ModelState.AddModelError("", "Không thể lưu danh mục; kiểm tra mã và tên trùng."); return View("Form", form); }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet, Authorize(Policy = "ManagerOnly")]
    public async Task<IActionResult> Edit(string id)
    {
        var category = await db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
        return category is null ? NotFound() : View("Form", new CategoryForm
        { Id = category.Id, Name = category.Name, Code = category.Code, DisplayOrder = category.DisplayOrder });
    }

    [HttpPost, Authorize(Policy = "ManagerOnly"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, CategoryForm form)
    {
        Normalize(form);
        if (id != form.Id) return BadRequest();
        if (!ModelState.IsValid) return View("Form", form);
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
        if (category is null) return NotFound();
        if (await db.Categories.AnyAsync(c => c.Id != id && (c.Code == form.Code || c.Name == form.Name)))
        {
            ModelState.AddModelError("", "Mã hoặc tên danh mục đã tồn tại.");
            return View("Form", form);
        }
        db.Entry(category).CurrentValues.SetValues(category with
        { Name = form.Name, Code = form.Code, DisplayOrder = form.DisplayOrder });
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException) { ModelState.AddModelError("", "Không thể cập nhật danh mục; kiểm tra mã và tên trùng."); return View("Form", form); }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet, Authorize(Policy = "ManagerOnly")]
    public async Task<IActionResult> Delete(string id)
    {
        var category = await db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
        return category is null ? NotFound() : View(category);
    }

    [HttpPost, ActionName("Delete"), Authorize(Policy = "ManagerOnly"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
        if (category is null) return NotFound();
        if (await db.Products.AnyAsync(p => p.CategoryId == id))
        {
            ModelState.AddModelError("", "Danh mục đang có sản phẩm, không thể xóa.");
            return View("Delete", category);
        }
        db.Entry(category).CurrentValues.SetValues(category with { IsActive = false });
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private void Normalize(CategoryForm form)
    {
        form.Id = form.Id?.Trim() ?? "";
        form.Name = form.Name?.Trim() ?? "";
        form.Code = form.Code?.Trim() ?? "";
        ModelState.Clear();
        TryValidateModel(form);
    }
}
