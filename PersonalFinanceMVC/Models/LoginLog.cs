using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceMVC.Models;

public class LoginLog
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "使用者代號")]
    public string Username { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "來源 IP")]
    public string IpAddress { get; set; } = string.Empty;

    [Display(Name = "登入時間")]
    public DateTime LoginAt { get; set; } = DateTime.Now;

    [Display(Name = "登入結果")]
    public bool IsSuccess { get; set; }

    [StringLength(200)]
    [Display(Name = "失敗原因")]
    public string? FailReason { get; set; }

    [Display(Name = "使用者")]
    public int? UserId { get; set; }
    public AppUser? User { get; set; }
}
