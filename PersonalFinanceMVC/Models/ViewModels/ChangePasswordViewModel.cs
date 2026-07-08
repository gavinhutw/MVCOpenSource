using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceMVC.Models.ViewModels;

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "請輸入目前密碼")]
    [DataType(DataType.Password)]
    [Display(Name = "目前密碼")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "請輸入新密碼")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "密碼長度至少 8 個字元")]
    [DataType(DataType.Password)]
    [Display(Name = "新密碼")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "請確認新密碼")]
    [Compare(nameof(NewPassword), ErrorMessage = "兩次輸入的密碼不一致")]
    [DataType(DataType.Password)]
    [Display(Name = "確認新密碼")]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>密碼已過期，強制修改</summary>
    public bool IsExpired { get; set; }
}
