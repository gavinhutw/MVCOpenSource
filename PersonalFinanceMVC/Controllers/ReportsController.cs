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

    [HttpGet]
    public IActionResult LargeExpense()
    {
        return View(new LargeExpenseViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> LargeExpense(decimal minAmount, string startMonth, string endMonth)
    {
        var now = DateTime.Now;

        // 解析 yyyy-MM 格式
        static (int y, int m) Parse(string s, DateTime fallback)
        {
            if (DateTime.TryParseExact(s + "-01", "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var dt))
                return (dt.Year, dt.Month);
            return (fallback.Year, fallback.Month);
        }

        var (sy, sm) = Parse(startMonth, now.AddMonths(-2));
        var (ey, em) = Parse(endMonth,   now);

        // 確保起始 <= 結束
        if (new DateTime(sy, sm, 1) > new DateTime(ey, em, 1))
            (sy, sm, ey, em) = (ey, em, sy, sm);

        var model = new LargeExpenseViewModel
        {
            MinAmount   = minAmount <= 0 ? 1000 : minAmount,
            StartMonth  = $"{sy:D4}-{sm:D2}",
            EndMonth    = $"{ey:D4}-{em:D2}",
            HasSearched = true,
            Groups      = await _reportService.GetLargeExpensesAsync(
                              minAmount <= 0 ? 1000 : minAmount, sy, sm, ey, em)
        };
        return View(model);
    }
}
