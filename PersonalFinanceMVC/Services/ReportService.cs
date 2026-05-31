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
}
