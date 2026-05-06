using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnterprisePortal.Models
{
    public class LeaveApprover
    {
        [Key]
        public int Id { get; set; }

        public int LeaveApplicationId { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "簽核主管")]
        public string ApproverId { get; set; } = string.Empty;

        [Display(Name = "簽核順序")]
        public int ApprovalOrder { get; set; }

        [Display(Name = "簽核狀態")]
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

        [StringLength(500)]
        [Display(Name = "簽核意見")]
        public string? Comment { get; set; }

        [Display(Name = "簽核時間")]
        public DateTime? ApprovedAt { get; set; }

        [ForeignKey("LeaveApplicationId")]
        public virtual LeaveApplication? LeaveApplication { get; set; }

        [ForeignKey("ApproverId")]
        public virtual Employee? Approver { get; set; }
    }

    public enum ApprovalStatus
    {
        [Display(Name = "待簽核")] Pending = 1,
        [Display(Name = "已同意")] Approved = 2,
        [Display(Name = "已拒絕")] Rejected = 3
    }
}
