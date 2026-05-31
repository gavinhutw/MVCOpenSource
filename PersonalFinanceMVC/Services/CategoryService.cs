using Microsoft.EntityFrameworkCore;
using PersonalFinanceMVC.Data;
using PersonalFinanceMVC.Models;

namespace PersonalFinanceMVC.Services;

public class CategoryService
{
    private readonly AppDbContext _db;

    public CategoryService(AppDbContext db) => _db = db;

    public async Task<List<Category>> GetAllAsync() =>
        await _db.Categories.OrderBy(c => c.Type).ThenBy(c => c.Name).ToListAsync();

    public async Task<List<Category>> GetByTypeAsync(TransactionType type) =>
        await _db.Categories.Where(c => c.Type == type).OrderBy(c => c.Name).ToListAsync();

    public async Task<Category?> GetByIdAsync(int id) =>
        await _db.Categories.FindAsync(id);

    public async Task<Category> CreateAsync(Category category)
    {
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return category;
    }

    public async Task UpdateAsync(Category category)
    {
        _db.Categories.Update(category);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null) return;
        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
    }
}
