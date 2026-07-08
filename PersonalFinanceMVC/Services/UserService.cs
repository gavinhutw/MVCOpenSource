using Microsoft.EntityFrameworkCore;
using PersonalFinanceMVC.Data;
using PersonalFinanceMVC.Models;

namespace PersonalFinanceMVC.Services;

public class UserService
{
    private readonly AppDbContext _db;
    private readonly AuthService _authService;

    public UserService(AppDbContext db, AuthService authService)
    {
        _db = db;
        _authService = authService;
    }

    public async Task<List<AppUser>> GetAllAsync() =>
        await _db.AppUsers.OrderBy(u => u.Username).ToListAsync();

    public async Task<AppUser?> GetByIdAsync(int id) =>
        await _db.AppUsers.FindAsync(id);

    public async Task<AppUser?> GetByUsernameAsync(string username) =>
        await _db.AppUsers.FirstOrDefaultAsync(u => u.Username == username);

    public async Task<bool> UsernameExistsAsync(string username, int excludeId = 0) =>
        await _db.AppUsers.AnyAsync(u => u.Username == username && u.Id != excludeId);

    public async Task<AppUser> CreateAsync(AppUser user, string plainPassword)
    {
        user.PasswordHash = _authService.HashPassword(plainPassword);
        user.CreatedAt = DateTime.Now;
        user.PasswordChangedAt = DateTime.Now;
        _db.AppUsers.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task UpdateAsync(AppUser user)
    {
        _db.AppUsers.Update(user);
        await _db.SaveChangesAsync();
    }

    public async Task ResetPasswordAsync(int userId, string newPassword)
    {
        var user = await _db.AppUsers.FindAsync(userId);
        if (user is null) return;
        user.PasswordHash = _authService.HashPassword(newPassword);
        user.PasswordChangedAt = DateTime.Now;
        await _db.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(int userId, string newPassword)
    {
        var user = await _db.AppUsers.FindAsync(userId);
        if (user is null) return;
        user.PasswordHash = _authService.HashPassword(newPassword);
        user.PasswordChangedAt = DateTime.Now;
        await _db.SaveChangesAsync();
    }

    public async Task ToggleActiveAsync(int userId)
    {
        var user = await _db.AppUsers.FindAsync(userId);
        if (user is null) return;
        user.IsActive = !user.IsActive;
        await _db.SaveChangesAsync();
    }

    public async Task RecordLoginAsync(string username, string ip, bool success,
        string? failReason = null, int? userId = null)
    {
        _db.LoginLogs.Add(new LoginLog
        {
            Username = username,
            IpAddress = ip,
            LoginAt = DateTime.Now,
            IsSuccess = success,
            FailReason = failReason,
            UserId = userId
        });

        if (success && userId.HasValue)
        {
            var user = await _db.AppUsers.FindAsync(userId.Value);
            if (user is not null) user.LastLoginAt = DateTime.Now;
        }

        await _db.SaveChangesAsync();
    }

    public async Task<List<LoginLog>> GetLoginLogsAsync(int? userId = null, int take = 100) =>
        await _db.LoginLogs
            .Where(l => !userId.HasValue || l.UserId == userId)
            .OrderByDescending(l => l.LoginAt)
            .Take(take)
            .Include(l => l.User)
            .ToListAsync();
}
