using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceMVC.Models.ViewModels;

public class TransactionFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "請輸入金額")]
    [Range(0.01, double.MaxValue, ErrorMessage = "金額必須大於 0")]
    [Display(Name = "金額")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "請選擇日期")]
    [DataType(DataType.Date)]
    [Display(Name = "日期")]
    public DateTime Date { get; set; } = DateTime.Today;

    [StringLength(200)]
    [Display(Name = "說明")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "類型")]
    public TransactionType Type { get; set; } = TransactionType.Expense;

    [Required(ErrorMessage = "請選擇分類")]
    [Display(Name = "分類")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "請選擇帳戶")]
    [Display(Name = "帳戶")]
    public int AccountId { get; set; }

    public List<Account> Accounts { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
}
