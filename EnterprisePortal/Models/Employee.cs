using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnterprisePortal.Models
{
    public class Employee
    {
        [Key]
        [StringLength(20)]
        [Display(Name = "員工編號")]
        public string EmployeeId { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "員工大頭照")]
        public string? PhotoPath { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "員工姓名")]
        public string FullName { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "英文姓名")]
        public string? EnglishName { get; set; }

        [Required]
        [Display(Name = "性別")]
        public GenderType Gender { get; set; }

        [StringLength(20)]
        [Display(Name = "身份證號")]
        public string? IdNumber { get; set; }

        [Display(Name = "婚姻狀況")]
        public MaritalStatus MaritalStatus { get; set; }

        [Display(Name = "最高學歷")]
        public EducationLevel EducationLevel { get; set; }

        [StringLength(200)]
        [Display(Name = "聯絡地址")]
        public string? Address { get; set; }

        [StringLength(30)]
        [Display(Name = "連絡電話")]
        public string? Phone { get; set; }

        [StringLength(20)]
        [Display(Name = "手機號碼")]
        public string? Mobile { get; set; }

        [StringLength(100)]
        [EmailAddress]
        [Display(Name = "電子郵件信箱")]
        public string? Email { get; set; }

        [Display(Name = "到職日期")]
        public DateTime? HireDate { get; set; }

        [Display(Name = "在職狀況")]
        public EmploymentStatus EmploymentStatus { get; set; } = EmploymentStatus.Active;

        [StringLength(20)]
        [Display(Name = "所屬部門")]
        public string? DepartmentId { get; set; }

        [StringLength(20)]
        [Display(Name = "直屬主管")]
        public string? SupervisorId { get; set; }

        [StringLength(50)]
        [Display(Name = "擔任職稱")]
        public string? JobTitle { get; set; }

        [Required]
        [StringLength(256)]
        [Display(Name = "登入密碼")]
        public string PasswordHash { get; set; } = string.Empty;

        [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }

        [ForeignKey("SupervisorId")]
        public virtual Employee? Supervisor { get; set; }

        public virtual ICollection<LeaveApplication> LeaveApplications { get; set; } = new List<LeaveApplication>();
        public virtual ICollection<OvertimeApplication> OvertimeApplications { get; set; } = new List<OvertimeApplication>();
        public virtual ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
    }

    public enum GenderType
    {
        [Display(Name = "男")] Male = 1,
        [Display(Name = "女")] Female = 2
    }

    public enum MaritalStatus
    {
        [Display(Name = "未婚")] Single = 1,
        [Display(Name = "已婚")] Married = 2,
        [Display(Name = "離婚")] Divorced = 3,
        [Display(Name = "喪偶")] Widowed = 4
    }

    public enum EducationLevel
    {
        [Display(Name = "國中以下")] BelowJuniorHigh = 1,
        [Display(Name = "高中職")] HighSchool = 2,
        [Display(Name = "大專")] Associate = 3,
        [Display(Name = "大學")] Bachelor = 4,
        [Display(Name = "碩士")] Master = 5,
        [Display(Name = "博士")] Doctor = 6
    }

    public enum EmploymentStatus
    {
        [Display(Name = "在職")] Active = 1,
        [Display(Name = "離職")] Resigned = 2,
        [Display(Name = "留職停薪")] LeaveWithoutPay = 3
    }
}
