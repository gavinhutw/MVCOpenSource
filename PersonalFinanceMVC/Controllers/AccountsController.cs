using Microsoft.AspNetCore.Mvc;
using PersonalFinanceMVC.Models;
using PersonalFinanceMVC.Services;

namespace PersonalFinanceMVC.Controllers;

public class AccountsController : Controller
{
    private readonly AccountService _accountService;

    public AccountsController(AccountService accountService) => _accountService = accountService;

    public async Task<IActionResult> Index()
    {
        var accounts = await _accountService.GetAllAsync();
        ViewBag.TotalBalance = accounts.Sum(a => a.Balance);
        return View(accounts);
    }

    public IActionResult Create() => View(new Account());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Account account)
    {
        if (!ModelState.IsValid) return View(account);
        await _accountService.CreateAsync(account);
        TempData["Success"] = "帳戶新增成功";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var account = await _accountService.GetByIdAsync(id);
        if (account is null) return NotFound();
        return View(account);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Account account)
    {
        if (id != account.Id) return BadRequest();
        if (!ModelState.IsValid) return View(account);
        account.IsActive = true;
        await _accountService.UpdateAsync(account);
        TempData["Success"] = "帳戶更新成功";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _accountService.SoftDeleteAsync(id);
        TempData["Success"] = "帳戶已刪除";
        return RedirectToAction(nameof(Index));
    }
}
