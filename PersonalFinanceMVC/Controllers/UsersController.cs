using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceMVC.Models;
using PersonalFinanceMVC.Models.ViewModels;
using PersonalFinanceMVC.Services;

namespace PersonalFinanceMVC.Controllers;

/// <summary>使用者管理（僅限系統管理員）</summary>
public class UsersController : Controller
{
    private readonly UserService _userService;
    private readonly AuthService _authService;

    public UsersController(UserService userService, AuthService authService)
    {
        _userService = userService;
        _authService = authService;
    }

    private bool IsAdmin => User.FindFirst("IsAdmin")?.Value == "True";

    private IActionResult ForbidAdmin()
    {
        TempData["Error"] = "此功能僅限系統管理員使用";
        return RedirectToAction("Index", "Home");
    }

    // ── 使用者清單 ────────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        if (!IsAdmin) return ForbidAdmin();
        var users = await _userService.GetAllAsync();
        return View(users);
    }

    // ── 新增使用者 ────────────────────────────────────────────
    public IActionResult Create()
    {
        if (!IsAdmin) return ForbidAdmin();
        return View(new UserFormViewModel { IsActive = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserFormViewModel model)
    {
        if (!IsAdmin) return ForbidAdmin();

        if (string.IsNullOrEmpty(model.Password))
            ModelState.AddModelError(nameof(model.Password), "新增使用者時密碼為必填");

        if (!ModelState.IsValid)
            return View(model);

        if (await _userService.UsernameExistsAsync(model.Username))
        {
            ModelState.AddModelError(nameof(model.Username), "此使用者代號已存在");
            return View(model);
        }

        var user = new AppUser
        {
            Username    = model.Username,
            DisplayName = model.DisplayName,
            Email       = model.Email,
            IsAdmin     = model.IsAdmin,
            IsActive    = model.IsActive,
        };

        await _userService.CreateAsync(user, model.Password!);
        TempData["Success"] = $"使用者「{user.Username}」建立成功";
        return RedirectToAction(nameof(Index));
    }

    // ── 編輯使用者 ────────────────────────────────────────────
    public async Task<IActionResult> Edit(int id)
    {
        if (!IsAdmin) return ForbidAdmin();

        var user = await _userService.GetByIdAsync(id);
        if (user is null) return NotFound();

        return View(new UserFormViewModel
        {
            Id                = user.Id,
            Username          = user.Username,
            DisplayName       = user.DisplayName,
            Email             = user.Email,
            IsAdmin           = user.IsAdmin,
            IsActive          = user.IsActive,
            PasswordChangedAt = user.PasswordChangedAt,
            LastLoginAt       = user.LastLoginAt,
            CreatedAt         = user.CreatedAt,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UserFormViewModel model)
    {
        if (!IsAdmin) return ForbidAdmin();
        if (id != model.Id) return BadRequest();

        // 密碼欄位：填了就檢查一致性
        if (!string.IsNullOrEmpty(model.Password) &&
            model.Password != model.ConfirmPassword)
        {
            ModelState.AddModelError(nameof(model.ConfirmPassword), "兩次輸入的密碼不一致");
        }

        // 移除不需要在 Edit 驗證的欄位
        ModelState.Remove(nameof(model.Password));
        ModelState.Remove(nameof(model.ConfirmPassword));

        if (!ModelState.IsValid)
            return View(model);

        var user = await _userService.GetByIdAsync(id);
        if (user is null) return NotFound();

        user.DisplayName = model.DisplayName;
        user.Email       = model.Email;
        user.IsAdmin     = model.IsAdmin;
        user.IsActive    = model.IsActive;

        await _userService.UpdateAsync(user);

        // 若有輸入新密碼則重設
        if (!string.IsNullOrEmpty(model.Password))
            await _userService.ResetPasswordAsync(id, model.Password);

        TempData["Success"] = $"使用者「{user.Username}」已更新";
        return RedirectToAction(nameof(Index));
    }

    // ── 啟用 / 停用 ──────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        if (!IsAdmin) return ForbidAdmin();
        await _userService.ToggleActiveAsync(id);
        TempData["Success"] = "帳號狀態已更新";
        return RedirectToAction(nameof(Index));
    }

    // ── 登入記錄 ──────────────────────────────────────────────
    public async Task<IActionResult> LoginLogs(int? userId)
    {
        if (!IsAdmin) return ForbidAdmin();

        var logs = await _userService.GetLoginLogsAsync(userId);
        ViewBag.FilterUserId = userId;
        ViewBag.Users = await _userService.GetAllAsync();
        return View(logs);
    }
}
