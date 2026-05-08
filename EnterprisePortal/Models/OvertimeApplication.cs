using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnterprisePortal.Models
{
    public class OvertimeApplication
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string ApplicantId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "加班起始日期")]
        public DateTime StartDate { get; set; }

        [Display(Name = "起始時間（時）")]
        public int? StartHour { get; set; }

        [Required]
        [Display(Name = "加班結束日期")]
        public DateTime EndDate { get; set; }

        [Display(Name = "結束時間（時）")]
        public int? EndHour { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "加班原因")]
        public string Reason { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "代理人")]
        public string? ProxyId { get; set; }

        [Display(Name = "申請狀態")]
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

        [Display(Name = "申請時間")]
        public DateTime AppliedAt { get; set; } = DateTime.Now;

        [StringLength(500)]
        [Display(Name = "備註")]
        public string? Remarks { get; set; }

        [ForeignKey("ApplicantId")]
        public virtual Employee? Applicant { get; set; }

        [ForeignKey("ProxyId")]
        public virtual Employee? Proxy { get; set; }

        public virtual ICollection<OvertimeApprover> Approvers { get; set; } = new List<OvertimeApprover>();
    }
}
