namespace PersonalFinanceMVC.Models.ViewModels;

public class ReportViewModel
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal MonthlyIncome { get; set; }
    public decimal MonthlyExpense { get; set; }
    public decimal MonthlyNet => MonthlyIncome - MonthlyExpense;
    public List<CategoryReport> CategoryReports { get; set; } = new();
    public List<MonthlyReport> MonthlyTrend { get; set; } = new();
}
