using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceMVC.Models.ViewModels;

public class TransactionFilterViewModel
{
    [DataType(DataType.Date)]
    [Display(Name = "開始日期")]
    public DateTime? StartDate { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "結束日期")]
    public DateTime? EndDate { get; set; }

    [Display(Name = "帳戶")]
    public int? AccountId { get; set; }

    [Display(Name = "分類")]
    public int? CategoryId { get; set; }

    [Display(Name = "類型")]
    public TransactionType? Type { get; set; }

    public List<Transaction> Transactions { get; set; } = new();
    public List<Account> Accounts { get; set; } = new();
    public List<Category> Categories { get; set; } = new();

    public decimal TotalIncome => Transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
    public decimal TotalExpense => Transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
    public decimal Net => TotalIncome - TotalExpense;
}
