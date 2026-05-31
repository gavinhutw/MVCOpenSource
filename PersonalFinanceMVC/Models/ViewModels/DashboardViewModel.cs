namespace PersonalFinanceMVC.Models.ViewModels;

public class DashboardViewModel
{
    public decimal TotalBalance { get; set; }
    public decimal MonthlyIncome { get; set; }
    public decimal MonthlyExpense { get; set; }
    public decimal MonthlyNet => MonthlyIncome - MonthlyExpense;
    public List<Transaction> RecentTransactions { get; set; } = new();
    public List<CategoryReport> CategoryReports { get; set; } = new();
    public List<MonthlyReport> MonthlyTrend { get; set; } = new();
}

public class CategoryReport
{
    public string CategoryName { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

public class MonthlyReport
{
    public int Month { get; set; }
    public string MonthLabel => $"{Month}月";
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
    public decimal Net => Income - Expense;
}
