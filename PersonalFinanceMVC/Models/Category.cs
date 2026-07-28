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

    [Display(Name = "預算金額")]
    [Range(0, int.MaxValue, ErrorMessage = "預算金額不可為負數")]
    public int Budget { get; set; } = 0;

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
