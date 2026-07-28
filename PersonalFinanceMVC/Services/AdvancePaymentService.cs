using Microsoft.EntityFrameworkCore;
using PersonalFinanceMVC.Data;
using PersonalFinanceMVC.Models;

namespace PersonalFinanceMVC.Services;

public class AdvancePaymentService
{
    private readonly AppDbContext _db;

    public AdvancePaymentService(AppDbContext db) => _db = db;

    public async Task<List<AdvancePayment>> GetAllAsync() =>
        await _db.AdvancePayments
            .Include(a => a.Category)
            .OrderBy(a => a.Category.Name)
            .ThenBy(a => a.SerialNo)
            .ToListAsync();

    public async Task<List<AdvancePayment>> GetByCategoryAsync(int categoryId) =>
        await _db.AdvancePayments
            .Where(a => a.Categories_Id == categoryId)
            .OrderBy(a => a.SerialNo)
            .ToListAsync();

    public async Task<AdvancePayment?> GetByIdAsync(int serialNo) =>
        await _db.AdvancePayments
            .Include(a => a.Category)
            .FirstOrDefaultAsync(a => a.SerialNo == serialNo);

    public async Task CreateAsync(AdvancePayment item)
    {
        _db.AdvancePayments.Add(item);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(AdvancePayment item)
    {
        _db.AdvancePayments.Update(item);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int serialNo)
    {
        var item = await _db.AdvancePayments.FindAsync(serialNo);
        if (item is null) return;
        _db.AdvancePayments.Remove(item);
        await _db.SaveChangesAsync();
    }
}
