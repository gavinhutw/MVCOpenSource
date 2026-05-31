using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceMVC.Models;

public class Account
{
    public int Id { get; set; }

    [Required(ErrorMessage = "請輸入帳戶名稱")]
    [StringLength(50)]
    [Display(Name = "帳戶名稱")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "帳戶類型")]
    public AccountType Type { get; set; }

    [Display(Name = "餘額")]
    public decimal Balance { get; set; }

    [Display(Name = "顏色")]
    public string Color { get; set; } = "#1976D2";

    public bool IsActive { get; set; } = true;

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}

public enum AccountType
{
    [Display(Name = "現金")] Cash,
    [Display(Name = "銀行帳戶")] Bank,
    [Display(Name = "信用卡")] CreditCard
}
