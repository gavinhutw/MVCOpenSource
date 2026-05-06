using System.ComponentModel.DataAnnotations;

namespace EnterprisePortal.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "請輸入員工編號")]
        [Display(Name = "員工編號")]
        public string EmployeeId { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入登入密碼")]
        [DataType(DataType.Password)]
        [Display(Name = "登入密碼")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入驗證碼")]
        [Display(Name = "驗證碼")]
        public string CaptchaInput { get; set; } = string.Empty;
    }
}
