using System.Security.Claims;
using CafePOSManagement.Web.DAL;
using CafePOSManagement.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafePOSManagement.Web.Controllers;

public sealed class LoginController : Controller
{
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

    [HttpGet]
    public IActionResult Denied() => View();
}
