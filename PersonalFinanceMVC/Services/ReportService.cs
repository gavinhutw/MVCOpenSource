using Microsoft.EntityFrameworkCore;
using PersonalFinanceMVC.Data;
using PersonalFinanceMVC.Models;
using PersonalFinanceMVC.Models.ViewModels;

namespace PersonalFinanceMVC.Services;

public class ReportService
{
    private readonly AppDbContext _db;

    public ReportService(AppDbContext db) => _db = db;

    public async Task<List<CategoryReport>> GetExpenseByCategoryAsync(int year, int month) =>
        await _db.Transactions
            .Where(t => t.Type == TransactionType.Expense && t.Date.Year == year && t.Date.Month == month)
            .Include(t => t.Category)
            .GroupBy(t => new { t.CategoryId, t.Category.Name, t.Category.Color })
            .Select(g => new CategoryReport
            {
                CategoryName = g.Key.Name,
                Color = g.Key.Color,
                Total = g.Sum(t => t.Amount)
            })
            .OrderByDescending(r => r.Total)
            .ToListAsync();

    public async Task<List<MonthlyReport>> GetMonthlyTrendAsync(int year)
    {
        var reports = new List<MonthlyReport>();
        for (int m = 1; m <= 12; m++)
        {
            var income = await _db.Transactions
                .Where(t => t.Type == TransactionType.Income && t.Date.Year == year && t.Date.Month == m)
                .SumAsync(t => (decimal?)t.Amount) ?? 0;
            var expense = await _db.Transactions
                .Where(t => t.Type == TransactionType.Expense && t.Date.Year == year && t.Date.Month == m)
                .SumAsync(t => (decimal?)t.Amount) ?? 0;
            reports.Add(new MonthlyReport { Month = m, Income = income, Expense = expense });
        }
        return reports;
    }

    public async Task<List<LargeExpenseMonthGroup>> GetLargeExpensesAsync(
        decimal minAmount, int startYear, int startMonth, int endYear, int endMonth)
    {
        var startDate = new DateTime(startYear, startMonth, 1);
        var endDate   = new DateTime(endYear,   endMonth,   1).AddMonths(1).AddDays(-1);

        var rows = await _db.Transactions
            .Where(t => t.Type       == TransactionType.Expense
                     && t.Amount     >= minAmount
                     && t.Date       >= startDate
                     && t.Date       <= endDate)
            .Include(t => t.Category)
            .Include(t => t.Account)
            .OrderBy(t => t.Date)
            .ThenByDescending(t => t.Amount)
            .Select(t => new LargeExpenseRow
            {
                Id            = t.Id,
                Date          = t.Date,
                Description   = t.Description,
                CategoryName  = t.Category.Name,
                CategoryColor = t.Category.Color,
                AccountName   = t.Account.Name,
                Amount        = t.Amount
            })
            .ToListAsync();

        return rows
            .GroupBy(r => new { r.Date.Year, r.Date.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new LargeExpenseMonthGroup
            {
                Year       = g.Key.Year,
                Month      = g.Key.Month,
                MonthTotal = g.Sum(r => r.Amount),
                Items      = g.ToList()
            })
            .ToList();
    }
}
