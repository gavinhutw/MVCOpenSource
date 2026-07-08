using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceMVC.Models;

public class AppUser
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "使用者代號")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [Display(Name = "密碼雜湊")]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "顯示名稱")]
    public string DisplayName { get; set; } = string.Empty;

    [StringLength(200)]
    [EmailAddress]
    [Display(Name = "電子郵件")]
    public string? Email { get; set; }

    [Display(Name = "系統管理員")]
    public bool IsAdmin { get; set; }

    [Display(Name = "帳號狀態")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "密碼變更時間")]
    public DateTime PasswordChangedAt { get; set; } = DateTime.Now;

    [Display(Name = "建立時間")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Display(Name = "最後登入時間")]
    public DateTime? LastLoginAt { get; set; }

    public ICollection<LoginLog> LoginLogs { get; set; } = new List<LoginLog>();
}
