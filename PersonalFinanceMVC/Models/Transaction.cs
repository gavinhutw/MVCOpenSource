using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceMVC.Models;

public class Transaction
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
    public TransactionType Type { get; set; }

    [Required(ErrorMessage = "請選擇分類")]
    [Display(Name = "分類")]
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    [Required(ErrorMessage = "請選擇帳戶")]
    [Display(Name = "帳戶")]
    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum TransactionType
{
    [Display(Name = "收入")] Income,
    [Display(Name = "支出")] Expense
}
