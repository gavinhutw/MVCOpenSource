using Microsoft.EntityFrameworkCore;
using PersonalFinanceMVC.Data;
using PersonalFinanceMVC.Models;

namespace PersonalFinanceMVC.Services;

public class TransactionService
{
    private readonly AppDbContext _db;

    public TransactionService(AppDbContext db) => _db = db;

    public async Task<List<Transaction>> GetFilteredAsync(
        DateTime? startDate, DateTime? endDate,
        int? accountId, int? categoryId, TransactionType? type)
    {
        var query = _db.Transactions
            .Include(t => t.Category)
            .Include(t => t.Account)
            .AsQueryable();

        if (startDate.HasValue) query = query.Where(t => t.Date >= startDate.Value);
        if (endDate.HasValue) query = query.Where(t => t.Date <= endDate.Value);
        if (accountId.HasValue) query = query.Where(t => t.AccountId == accountId.Value);
        if (categoryId.HasValue) query = query.Where(t => t.CategoryId == categoryId.Value);
        if (type.HasValue) query = query.Where(t => t.Type == type.Value);

        return await query.OrderByDescending(t => t.Date).ThenByDescending(t => t.CreatedAt).ToListAsync();
    }

    public async Task<List<Transaction>> GetRecentAsync(int count = 10) =>
        await _db.Transactions
            .Include(t => t.Category)
            .Include(t => t.Account)
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.CreatedAt)
            .Take(count)
            .ToListAsync();

    public async Task<Transaction?> GetByIdAsync(int id) =>
        await _db.Transactions
            .Include(t => t.Category)
            .Include(t => t.Account)
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<Transaction> CreateAsync(Transaction transaction)
    {
        _db.Transactions.Add(transaction);

        var account = await _db.Accounts.FindAsync(transaction.AccountId);
        if (account is not null)
            account.Balance += transaction.Type == TransactionType.Income ? transaction.Amount : -transaction.Amount;

        await _db.SaveChangesAsync();
        return transaction;
    }

    public async Task UpdateAsync(Transaction updated)
    {
        var original = await _db.Transactions.AsNoTracking().FirstOrDefaultAsync(t => t.Id == updated.Id);
        if (original is null) return;

        var oldAccount = await _db.Accounts.FindAsync(original.AccountId);
        if (oldAccount is not null)
            oldAccount.Balance -= original.Type == TransactionType.Income ? original.Amount : -original.Amount;

        var newAccount = await _db.Accounts.FindAsync(updated.AccountId);
        if (newAccount is not null)
            newAccount.Balance += updated.Type == TransactionType.Income ? updated.Amount : -updated.Amount;

        _db.Transactions.Update(updated);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var transaction = await _db.Transactions.FindAsync(id);
        if (transaction is null) return;

        var account = await _db.Accounts.FindAsync(transaction.AccountId);
        if (account is not null)
            account.Balance -= transaction.Type == TransactionType.Income ? transaction.Amount : -transaction.Amount;

        _db.Transactions.Remove(transaction);
        await _db.SaveChangesAsync();
    }

    public async Task<decimal> GetMonthlyIncomeAsync(int year, int month) =>
        await _db.Transactions
            .Where(t => t.Type == TransactionType.Income && t.Date.Year == year && t.Date.Month == month)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

    public async Task<decimal> GetMonthlyExpenseAsync(int year, int month) =>
        await _db.Transactions
            .Where(t => t.Type == TransactionType.Expense && t.Date.Year == year && t.Date.Month == month)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

    public async Task<decimal> GetMonthlyExpenseByCategoryAsync(int year, int month, int categoryId) =>
        await _db.Transactions
            .Where(t => t.Type == TransactionType.Expense
                     && t.Date.Year == year
                     && t.Date.Month == month
                     && t.CategoryId == categoryId)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;
}
