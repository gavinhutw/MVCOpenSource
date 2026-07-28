using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersonalFinanceMVC.Models;

public class AdvancePayment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Display(Name = "序號")]
    public int SerialNo { get; set; }

    [Required(ErrorMessage = "請選擇分類")]
    [Display(Name = "分類")]
    public int Categories_Id { get; set; }

    [ForeignKey("Categories_Id")]
    public Category Category { get; set; } = null!;

    [Required(ErrorMessage = "請輸入預支項目說明")]
    [StringLength(100, ErrorMessage = "說明不可超過 100 個字元")]
    [Display(Name = "預支項目說明")]
    public string Name { get; set; } = "";

    [Required]
    [Range(1, 12, ErrorMessage = "計費月份個數須在 1 到 12 之間")]
    [Display(Name = "計費月份個數")]
    public int MonthCount { get; set; } = 1;

    [Display(Name = "預支金額")]
    [Range(0, int.MaxValue, ErrorMessage = "預支金額不可為負數")]
    public int Amount { get; set; } = 0;
}
