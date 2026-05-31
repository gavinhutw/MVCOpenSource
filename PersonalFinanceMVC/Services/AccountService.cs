using Microsoft.EntityFrameworkCore;
using PersonalFinanceMVC.Data;
using PersonalFinanceMVC.Models;

namespace PersonalFinanceMVC.Services;

public class AccountService
{
    private readonly AppDbContext _db;

    public AccountService(AppDbContext db) => _db = db;

    public async Task<List<Account>> GetAllAsync() =>
        await _db.Accounts.Where(a => a.IsActive).OrderBy(a => a.Name).ToListAsync();

    public async Task<Account?> GetByIdAsync(int id) =>
        await _db.Accounts.FindAsync(id);

    public async Task<Account> CreateAsync(Account account)
    {
        _db.Accounts.Add(account);
        await _db.SaveChangesAsync();
        return account;
    }

    public async Task UpdateAsync(Account account)
    {
        _db.Accounts.Update(account);
        await _db.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(int id)
    {
        var account = await _db.Accounts.FindAsync(id);
        if (account is null) return;
        account.IsActive = false;
        await _db.SaveChangesAsync();
    }

    public async Task<decimal> GetTotalBalanceAsync() =>
        await _db.Accounts.Where(a => a.IsActive).SumAsync(a => a.Balance);
}
