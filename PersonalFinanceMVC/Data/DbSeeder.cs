using PersonalFinanceMVC.Models;
using PersonalFinanceMVC.Services;

namespace PersonalFinanceMVC.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        // 建立預設管理員帳號
        if (!db.AppUsers.Any())
        {
            var authService = new AuthService();
            db.AppUsers.Add(new AppUser
            {
                Username = "admin",
                PasswordHash = authService.HashPassword("Admin@123"),
                DisplayName = "系統管理員",
                Email = "admin@localhost",
                IsAdmin = true,
                IsActive = true,
                PasswordChangedAt = DateTime.Now,
                CreatedAt = DateTime.Now
            });
            db.SaveChanges();
        }

        if (db.Accounts.Any()) return;

        db.Accounts.AddRange(
            new Account { Name = "現金", Type = AccountType.Cash, Balance = 0, Color = "#4CAF50" },
            new Account { Name = "銀行帳戶", Type = AccountType.Bank, Balance = 0, Color = "#2196F3" },
            new Account { Name = "信用卡", Type = AccountType.CreditCard, Balance = 0, Color = "#F44336" }
        );

        db.Categories.AddRange(
            new Category { Name = "餐飲", Type = TransactionType.Expense, Color = "#FF5722" },
            new Category { Name = "交通", Type = TransactionType.Expense, Color = "#607D8B" },
            new Category { Name = "購物", Type = TransactionType.Expense, Color = "#E91E63" },
            new Category { Name = "娛樂", Type = TransactionType.Expense, Color = "#9C27B0" },
            new Category { Name = "醫療", Type = TransactionType.Expense, Color = "#F44336" },
            new Category { Name = "住房", Type = TransactionType.Expense, Color = "#795548" },
            new Category { Name = "教育", Type = TransactionType.Expense, Color = "#3F51B5" },
            new Category { Name = "其他支出", Type = TransactionType.Expense, Color = "#9E9E9E" },
            new Category { Name = "薪資", Type = TransactionType.Income, Color = "#4CAF50" },
            new Category { Name = "獎金", Type = TransactionType.Income, Color = "#FFC107" },
            new Category { Name = "投資", Type = TransactionType.Income, Color = "#00BCD4" },
            new Category { Name = "其他收入", Type = TransactionType.Income, Color = "#8BC34A" }
        );

        db.SaveChanges();
    }
}
