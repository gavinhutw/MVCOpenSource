using Microsoft.AspNetCore.Mvc;
using PersonalFinanceMVC.Models.ViewModels;
using PersonalFinanceMVC.Services;

namespace PersonalFinanceMVC.Controllers;

public class HomeController : Controller
{
    private readonly AccountService _accountService;
    private readonly TransactionService _transactionService;
    private readonly ReportService _reportService;

    public HomeController(AccountService accountService, TransactionService transactionService, ReportService reportService)
    {
        _accountService = accountService;
        _transactionService = transactionService;
        _reportService = reportService;
    }

    public async Task<IActionResult> Index()
    {
        var now = DateTime.Now;
        var model = new DashboardViewModel
        {
            TotalBalance = await _accountService.GetTotalBalanceAsync(),
            MonthlyIncome = await _transactionService.GetMonthlyIncomeAsync(now.Year, now.Month),
            MonthlyExpense = await _transactionService.GetMonthlyExpenseAsync(now.Year, now.Month),
            RecentTransactions = await _transactionService.GetRecentAsync(10),
            CategoryReports = await _reportService.GetExpenseByCategoryAsync(now.Year, now.Month),
            MonthlyTrend = await _reportService.GetMonthlyTrendAsync(now.Year)
        };
        return View(model);
    }
}
