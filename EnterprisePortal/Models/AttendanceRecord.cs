using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnterprisePortal.Models
{
    public class AttendanceRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string EmployeeId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "刷卡類別")]
        public ClockType ClockType { get; set; }

        [Required]
        [Display(Name = "體溫")]
        [Range(35.0, 42.0, ErrorMessage = "體溫須介於 35.0℃ 至 42.0℃ 之間")]
        public decimal Temperature { get; set; }

        [Display(Name = "刷卡時間")]
        public DateTime RecordedAt { get; set; } = DateTime.Now;

        [StringLength(50)]
        [Display(Name = "IP位址")]
        public string? IpAddress { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }
    }

    public enum ClockType
    {
        [Display(Name = "上班刷卡")] ClockIn = 1,
        [Display(Name = "下班刷卡")] ClockOut = 2
    }
}
