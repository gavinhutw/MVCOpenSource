using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceMVC.Models.ViewModels;
using PersonalFinanceMVC.Services;
using System.Security.Claims;

namespace PersonalFinanceMVC.Controllers;

public class AuthController : Controller
{
    private readonly UserService _userService;
    private readonly AuthService _authService;

    public AuthController(UserService userService, AuthService authService)
    {
        _userService = userService;
        _authService = authService;
    }

    // ── 登入 ────────────────────────────────────────────────────
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToLocal(returnUrl);

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var ip = AuthService.GetClientIp(HttpContext);

        // 1. 驗證碼檢查
        if (!CaptchaController.Validate(HttpContext.Session, model.CaptchaInput))
        {
            CaptchaController.Clear(HttpContext.Session);
            await _userService.RecordLoginAsync(model.Username, ip, false, "驗證碼錯誤");
            ModelState.AddModelError(nameof(model.CaptchaInput), "驗證碼錯誤，請重新輸入");
            return View(model);
        }
        CaptchaController.Clear(HttpContext.Session);

        // 2. 查找使用者
        var user = await _userService.GetByUsernameAsync(model.Username);
        if (user is null)
        {
            await _userService.RecordLoginAsync(model.Username, ip, false, "使用者不存在");
            ModelState.AddModelError(string.Empty, "使用者代號或密碼錯誤");
            return View(model);
        }

        // 3. 帳號停用
        if (!user.IsActive)
        {
            await _userService.RecordLoginAsync(model.Username, ip, false, "帳號已停用", user.Id);
            ModelState.AddModelError(string.Empty, "此帳號已停用，請聯絡管理員");
            return View(model);
        }

        // 4. 密碼驗證
        if (!_authService.VerifyPassword(model.Password, user.PasswordHash))
        {
            await _userService.RecordLoginAsync(model.Username, ip, false, "密碼錯誤", user.Id);
            ModelState.AddModelError(string.Empty, "使用者代號或密碼錯誤");
            return View(model);
        }

        // 5. 密碼到期檢查
        bool isExpired = _authService.IsPasswordExpired(user.PasswordChangedAt);

        // 6. 記錄登入成功
        await _userService.RecordLoginAsync(model.Username, ip, true, null, user.Id);

        // 7. 建立 Claims
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name,           user.Username),
            new("DisplayName",             user.DisplayName),
            new("IsAdmin",                 user.IsAdmin.ToString()),
            new("PasswordChangedAt",       user.PasswordChangedAt.Ticks.ToString()),
            new("PasswordExpired",         isExpired.ToString()),
        };

        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc   = DateTimeOffset.UtcNow.AddHours(8)
            });

        if (isExpired)
            return RedirectToAction("ChangePassword", new { expired = true });

        return RedirectToLocal(model.ReturnUrl);
    }

    // ── 登出 ────────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    // ── 修改密碼 ─────────────────────────────────────────────────
    [HttpGet]
    public IActionResult ChangePassword(bool expired = false)
    {
        var isExpired = expired ||
            User.FindFirst("PasswordExpired")?.Value == "True";

        return View(new ChangePasswordViewModel { IsExpired = isExpired });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var user   = await _userService.GetByIdAsync(userId);
        if (user is null) return RedirectToAction("Login");

        // 驗證目前密碼
        if (!_authService.VerifyPassword(model.CurrentPassword, user.PasswordHash))
        {
            ModelState.AddModelError(nameof(model.CurrentPassword), "目前密碼錯誤");
            return View(model);
        }

        // 新密碼不能與舊密碼相同
        if (_authService.VerifyPassword(model.NewPassword, user.PasswordHash))
        {
            ModelState.AddModelError(nameof(model.NewPassword), "新密碼不能與目前密碼相同");
            return View(model);
        }

        // 更新密碼
        await _userService.ChangePasswordAsync(userId, model.NewPassword);

        // 重新核發 Cookie（移除 PasswordExpired claim）
        var updatedUser = await _userService.GetByIdAsync(userId);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, updatedUser!.Id.ToString()),
            new(ClaimTypes.Name,           updatedUser.Username),
            new("DisplayName",             updatedUser.DisplayName),
            new("IsAdmin",                 updatedUser.IsAdmin.ToString()),
            new("PasswordChangedAt",       updatedUser.PasswordChangedAt.Ticks.ToString()),
            new("PasswordExpired",         "False"),
        };

        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });

        TempData["Success"] = "密碼已成功變更";
        return RedirectToAction("Index", "Home");
    }

    // ── 工具方法 ──────────────────────────────────────────────────
    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }
}
