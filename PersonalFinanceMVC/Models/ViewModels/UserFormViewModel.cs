using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceMVC.Models.ViewModels;

public class UserFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "請輸入使用者代號")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "使用者代號長度 3~50 字元")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "只允許英文、數字及底線")]
    [Display(Name = "使用者代號")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "請輸入顯示名稱")]
    [StringLength(100)]
    [Display(Name = "顯示名稱")]
    public string DisplayName { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "電子郵件格式不正確")]
    [StringLength(200)]
    [Display(Name = "電子郵件")]
    public string? Email { get; set; }

    [Display(Name = "系統管理員")]
    public bool IsAdmin { get; set; }

    [Display(Name = "帳號啟用")]
    public bool IsActive { get; set; } = true;

    // 新增時必填，編輯時選填（留空則不更新）
    [StringLength(100, MinimumLength = 8, ErrorMessage = "密碼長度至少 8 個字元")]
    [DataType(DataType.Password)]
    [Display(Name = "密碼")]
    public string? Password { get; set; }

    [Compare(nameof(Password), ErrorMessage = "兩次輸入的密碼不一致")]
    [DataType(DataType.Password)]
    [Display(Name = "確認密碼")]
    public string? ConfirmPassword { get; set; }

    // 顯示用（編輯時）
    public DateTime? PasswordChangedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public bool IsEdit => Id > 0;

    public int PasswordAgeDays =>
        PasswordChangedAt.HasValue ? (int)(DateTime.Now - PasswordChangedAt.Value).TotalDays : 0;
}
