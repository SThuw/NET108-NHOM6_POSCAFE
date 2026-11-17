using System.Security.Claims;
using CafePOSManagement.Web.Data;
using CafePOSManagement.Web.Models.Entities;
using CafePOSManagement.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafePOSManagement.Web.Controllers;

[Authorize(Policy = "ManagerOnly")]
public sealed class EmployeesController(CafePosDbContext db) : Controller
{
    private static readonly PasswordHasher<User> PasswordHasher = new();

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var employees = await db.Users.AsNoTracking()
            .OrderByDescending(user => user.IsActive)
            .ThenBy(user => user.FullName)
            .Select(user => new EmployeeListItem(
                user.Id,
                user.FullName,
                user.Email,
                user.Phone,
                user.Role,
                user.IsActive,
                user.CreatedAt))
            .ToListAsync();

        return View(employees);
    }

    [HttpGet]
    public IActionResult Create() => View(new CreateEmployeeViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEmployeeViewModel form)
    {
        Normalize(form);
        if (!ModelState.IsValid) return View(form);

        if (await db.Users.AnyAsync(user => user.Email == form.Email))
        {
            ModelState.AddModelError(nameof(form.Email), "Email này đã được sử dụng.");
            return View(form);
        }

        var account = new User(
            $"emp-{Guid.NewGuid():N}",
            form.Email,
            "",
            form.FullName,
            form.Phone,
            form.IsManager,
            true);
        account = account with { Password = PasswordHasher.HashPassword(account, form.Password) };
        db.Users.Add(account);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "Không thể thêm nhân viên. Hãy kiểm tra email và thử lại.");
            return View(form);
        }

        TempData["EmployeesMessage"] = $"Đã thêm tài khoản cho {account.FullName}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();
        var account = await db.Users.AsNoTracking().FirstOrDefaultAsync(user => user.Id == id);
        return account is null
            ? NotFound()
            : View(new EditEmployeeViewModel
            {
                Id = account.Id,
                Email = account.Email,
                FullName = account.FullName,
                Phone = account.Phone,
                IsManager = account.Role,
                IsActive = account.IsActive
            });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, EditEmployeeViewModel form)
    {
        if (string.IsNullOrWhiteSpace(id) || id != form.Id) return BadRequest();
        var account = await db.Users.FirstOrDefaultAsync(user => user.Id == id);
        if (account is null) return NotFound();

        Normalize(form);
        if (!ModelState.IsValid) return View(form);

        if (await db.Users.AnyAsync(user => user.Id != id && user.Email == form.Email))
        {
            ModelState.AddModelError(nameof(form.Email), "Email này đã được sử dụng.");
            return View(form);
        }

        if (!CanChangeAccess(account, form.IsManager, form.IsActive, out var error))
        {
            ModelState.AddModelError("", error);
            return View(form);
        }

        if (account.Role && account.IsActive && (!form.IsManager || !form.IsActive) &&
            !await db.Users.AnyAsync(user => user.Id != id && user.Role && user.IsActive))
        {
            ModelState.AddModelError("", "Phải giữ lại ít nhất một tài khoản quản lý đang hoạt động.");
            return View(form);
        }

        db.Entry(account).CurrentValues.SetValues(account with
        {
            Email = form.Email,
            FullName = form.FullName,
            Phone = form.Phone,
            Role = form.IsManager,
            IsActive = form.IsActive
        });
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "Không thể cập nhật nhân viên. Hãy kiểm tra email và thử lại.");
            return View(form);
        }

        TempData["EmployeesMessage"] = $"Đã cập nhật tài khoản {account.Email}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();
        var account = await db.Users.FirstOrDefaultAsync(user => user.Id == id);
        if (account is null) return NotFound();
        if (!account.IsActive)
        {
            TempData["EmployeesMessage"] = $"Tài khoản {account.Email} đã bị khóa trước đó.";
            return RedirectToAction(nameof(Index));
        }

        if (!CanChangeAccess(account, account.Role, false, out var error))
        {
            TempData["EmployeesError"] = error;
            return RedirectToAction(nameof(Index));
        }

        if (account.Role && !await db.Users.AnyAsync(user => user.Id != id && user.Role && user.IsActive))
        {
            TempData["EmployeesError"] = "Không thể khóa tài khoản quản lý đang hoạt động cuối cùng.";
            return RedirectToAction(nameof(Index));
        }

        db.Entry(account).CurrentValues.SetValues(account with { IsActive = false });
        await db.SaveChangesAsync();
        TempData["EmployeesMessage"] = $"Đã khóa tài khoản {account.Email}.";
        return RedirectToAction(nameof(Index));
    }

    private bool CanChangeAccess(User account, bool isManager, bool isActive, out string error)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (account.Id == currentUserId && (!isManager || !isActive))
        {
            error = "Bạn không thể tự hạ quyền hoặc khóa tài khoản đang đăng nhập.";
            return false;
        }

        error = "";
        return true;
    }

    private void Normalize(CreateEmployeeViewModel form)
    {
        form.FullName = form.FullName?.Trim() ?? "";
        form.Email = form.Email?.Trim().ToLowerInvariant() ?? "";
        form.Phone = form.Phone?.Trim() ?? "";
        ModelState.Clear();
        TryValidateModel(form);
    }

    private void Normalize(EditEmployeeViewModel form)
    {
        form.FullName = form.FullName?.Trim() ?? "";
        form.Email = form.Email?.Trim().ToLowerInvariant() ?? "";
        form.Phone = form.Phone?.Trim() ?? "";
        ModelState.Clear();
        TryValidateModel(form);
    }
}
