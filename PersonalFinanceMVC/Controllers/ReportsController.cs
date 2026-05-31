using Microsoft.AspNetCore.Mvc;
using PersonalFinanceMVC.Models.ViewModels;
using PersonalFinanceMVC.Services;

namespace PersonalFinanceMVC.Controllers;

public class ReportsController : Controller
{
    private readonly ReportService _reportService;
    private readonly TransactionService _transactionService;

    public ReportsController(ReportService reportService, TransactionService transactionService)
    {
        _reportService = reportService;
        _transactionService = transactionService;
    }

    public async Task<IActionResult> Index(int? year, int? month)
    {
        var now = DateTime.Now;
        var selectedYear = year ?? now.Year;
        var selectedMonth = month ?? now.Month;

        var model = new ReportViewModel
        {
            Year = selectedYear,
            Month = selectedMonth,
            MonthlyIncome = await _transactionService.GetMonthlyIncomeAsync(selectedYear, selectedMonth),
            MonthlyExpense = await _transactionService.GetMonthlyExpenseAsync(selectedYear, selectedMonth),
            CategoryReports = await _reportService.GetExpenseByCategoryAsync(selectedYear, selectedMonth),
            MonthlyTrend = await _reportService.GetMonthlyTrendAsync(selectedYear)
        };
        return View(model);
    }
}
