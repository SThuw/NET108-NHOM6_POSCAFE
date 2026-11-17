using System.Security.Claims;
using System.Security.Cryptography;
using CafePOSManagement.Web.DAL;
using CafePOSManagement.Web.Data;
using CafePOSManagement.Web.Models.Entities;
using CafePOSManagement.Web.Models.ViewModels;
using CafePOSManagement.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;

namespace CafePOSManagement.Web.Controllers;

public sealed class LoginController(
    CafePosDbContext db,
    IConfiguration configuration,
    ILogger<LoginController> logger) : Controller
{
    private static readonly PasswordHasher<User> PasswordHasher = new();
    private readonly EmailService emailService = new(configuration);

    [HttpGet, AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/");
        return View(new LoginViewModel { ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : "/" });
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = UserDAL.Login(model.Email.Trim(), model.Password);
        if (user is null)
        {
            ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
            return View(model);
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role ? "Manager" : "Employee")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = model.RememberMe });
        return LocalRedirect(Url.IsLocalUrl(model.ReturnUrl) ? model.ReturnUrl! : "/");
    }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet, AllowAnonymous, ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken, EnableRateLimiting("PasswordReset"), ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        model.Email = model.Email?.Trim().ToLowerInvariant() ?? "";
        ModelState.Clear();
        TryValidateModel(model);
        if (!ModelState.IsValid) return View(model);

        if (!emailService.IsConfigured)
        {
            ModelState.AddModelError("", "Dịch vụ email chưa được cấu hình. Mật khẩu hiện tại chưa thay đổi.");
            return View(model);
        }

        var account = await db.Users.FirstOrDefaultAsync(user => user.Email == model.Email && user.IsActive);
        if (account is null)
        {
            TempData["ResetMessage"] = "Nếu email tồn tại và đang hoạt động, hệ thống đã gửi mật khẩu mới.";
            return RedirectToAction(nameof(ForgotPassword));
        }

        var originalPasswordHash = account.Password;
        // shortcut: LAB 5 gửi mật khẩu tạm qua email; hệ thống thật nên dùng reset token một lần có hạn.
        var newPassword = $"Poly@{Convert.ToHexString(RandomNumberGenerator.GetBytes(6))}";
        var newPasswordHash = PasswordHasher.HashPassword(account, newPassword);
        db.Entry(account).CurrentValues.SetValues(account with { Password = newPasswordHash });

        try
        {
            await db.SaveChangesAsync();
            await emailService.SendResetPasswordAsync(account.Email, account.FullName, newPassword);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Không thể cấp lại mật khẩu qua email cho tài khoản {UserId}.", account.Id);
            try
            {
                db.Entry(account).CurrentValues.SetValues(account with { Password = originalPasswordHash });
                await db.SaveChangesAsync();
            }
            catch (Exception restoreException)
            {
                logger.LogCritical(restoreException, "Không thể khôi phục mật khẩu cũ cho tài khoản {UserId}.", account.Id);
            }

            ModelState.AddModelError("", "Không thể gửi email lúc này. Vui lòng thử lại sau.");
            return View(model);
        }

        TempData["ResetMessage"] = "Nếu email tồn tại và đang hoạt động, hệ thống đã gửi mật khẩu mới.";
        return RedirectToAction(nameof(ForgotPassword));
    }

    [HttpGet, Authorize, ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Profile()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var account = string.IsNullOrWhiteSpace(id) ? null : UserDAL.GetById(id);
        if (account is null || !account.Active)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }
        return View(new UserProfileViewModel(account.Id, account.FullName, account.Email, account.Phone,
            account.Role ? "Quản lý" : "Nhân viên"));
    }

    [HttpGet, Authorize, ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [HttpPost, Authorize, ValidateAntiForgeryToken, ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var account = string.IsNullOrWhiteSpace(id)
            ? null
            : await db.Users.FirstOrDefaultAsync(user => user.Id == id && user.IsActive);
        if (account is null)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        if (PasswordHasher.VerifyHashedPassword(account, account.Password, model.CurrentPassword) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(nameof(model.CurrentPassword), "Mật khẩu hiện tại không đúng.");
            return View(model);
        }

        if (model.CurrentPassword == model.NewPassword)
        {
            ModelState.AddModelError(nameof(model.NewPassword), "Mật khẩu mới phải khác mật khẩu hiện tại.");
            return View(model);
        }

        db.Entry(account).CurrentValues.SetValues(account with
        {
            Password = PasswordHasher.HashPassword(account, model.NewPassword)
        });
        await db.SaveChangesAsync();
        TempData["ProfileMessage"] = "Đã đổi mật khẩu thành công.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    public IActionResult Denied()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }
}
