using Microsoft.AspNetCore.Mvc;
using PersonalFinanceMVC.Models;
using PersonalFinanceMVC.Models.ViewModels;
using PersonalFinanceMVC.Services;

namespace PersonalFinanceMVC.Controllers;

public class TransactionsController : Controller
{
    private readonly TransactionService _transactionService;
    private readonly AccountService _accountService;
    private readonly CategoryService _categoryService;

    public TransactionsController(TransactionService transactionService, AccountService accountService, CategoryService categoryService)
    {
        _transactionService = transactionService;
        _accountService = accountService;
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, int? accountId, int? categoryId, TransactionType? type)
    {
        if (startDate is null && endDate is null)
        {
            var now = DateTime.Now;
            startDate = new DateTime(now.Year, now.Month, 1);
            endDate = DateTime.Today;
        }

        var model = new TransactionFilterViewModel
        {
            StartDate = startDate,
            EndDate = endDate,
            AccountId = accountId,
            CategoryId = categoryId,
            Type = type,
            Transactions = await _transactionService.GetFilteredAsync(startDate, endDate, accountId, categoryId, type),
            Accounts = await _accountService.GetAllAsync(),
            Categories = await _categoryService.GetAllAsync()
        };
        return View(model);
    }

    public async Task<IActionResult> Create()
    {
        var model = new TransactionFormViewModel
        {
            Accounts = await _accountService.GetAllAsync(),
            Categories = await _categoryService.GetAllAsync()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TransactionFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Accounts = await _accountService.GetAllAsync();
            model.Categories = await _categoryService.GetAllAsync();
            return View(model);
        }

        var transaction = new Transaction
        {
            Amount = model.Amount,
            Date = model.Date,
            Description = model.Description,
            Type = model.Type,
            CategoryId = model.CategoryId,
            AccountId = model.AccountId
        };

        await _transactionService.CreateAsync(transaction);
        TempData["Success"] = "交易新增成功";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var transaction = await _transactionService.GetByIdAsync(id);
        if (transaction is null) return NotFound();

        var model = new TransactionFormViewModel
        {
            Id = transaction.Id,
            Amount = transaction.Amount,
            Date = transaction.Date,
            Description = transaction.Description,
            Type = transaction.Type,
            CategoryId = transaction.CategoryId,
            AccountId = transaction.AccountId,
            Accounts = await _accountService.GetAllAsync(),
            Categories = await _categoryService.GetAllAsync()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TransactionFormViewModel model)
    {
        if (id != model.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            model.Accounts = await _accountService.GetAllAsync();
            model.Categories = await _categoryService.GetAllAsync();
            return View(model);
        }

        var transaction = new Transaction
        {
            Id = model.Id,
            Amount = model.Amount,
            Date = model.Date,
            Description = model.Description,
            Type = model.Type,
            CategoryId = model.CategoryId,
            AccountId = model.AccountId
        };

        await _transactionService.UpdateAsync(transaction);
        TempData["Success"] = "交易更新成功";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _transactionService.DeleteAsync(id);
        TempData["Success"] = "交易已刪除";
        return RedirectToAction(nameof(Index));
    }
}
