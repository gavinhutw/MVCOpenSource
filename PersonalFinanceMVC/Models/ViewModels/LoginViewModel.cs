using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceMVC.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "請輸入使用者代號")]
    [Display(Name = "使用者代號")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "請輸入密碼")]
    [DataType(DataType.Password)]
    [Display(Name = "密碼")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "請輸入驗證碼")]
    [Display(Name = "驗證碼")]
    public string CaptchaInput { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
