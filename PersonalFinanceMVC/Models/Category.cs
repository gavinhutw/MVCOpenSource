using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceMVC.Models;

public class Category
{
    public int Id { get; set; }

    [Required(ErrorMessage = "請輸入分類名稱")]
    [StringLength(50)]
    [Display(Name = "分類名稱")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "類型")]
    public TransactionType Type { get; set; }

    [Display(Name = "顏色")]
    public string Color { get; set; } = "#1976D2";

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
