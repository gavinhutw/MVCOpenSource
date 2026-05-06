using System.ComponentModel.DataAnnotations;
using EnterprisePortal.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EnterprisePortal.ViewModels
{
    public class LeaveApplicationViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "請選擇請假類別")]
        [Display(Name = "請假類別")]
        public LeaveType LeaveType { get; set; }

        [Required(ErrorMessage = "請選擇起始日期")]
        [Display(Name = "請假起始日期")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "請選擇結束日期")]
        [Display(Name = "請假結束日期")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "請輸入請假原因")]
        [StringLength(500)]
        [Display(Name = "請假原因")]
        public string Reason { get; set; } = string.Empty;

        [Required(ErrorMessage = "請選擇代理人")]
        [Display(Name = "代理人")]
        public string ProxyId { get; set; } = string.Empty;

        [Display(Name = "簽核主管 (依序)")]
        public List<string> ApproverIds { get; set; } = new List<string>();

        [Display(Name = "附件")]
        public IFormFile? Attachment { get; set; }

        public SelectList? LeaveTypeList { get; set; }
        public SelectList? EmployeeList { get; set; }
    }

    public class LeaveApprovalViewModel
    {
        public int ApplicationId { get; set; }
        public string ApplicantName { get; set; } = string.Empty;
        public LeaveType LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? AttachmentPath { get; set; }
        public ApplicationStatus Status { get; set; }
        public DateTime AppliedAt { get; set; }

        [Display(Name = "簽核意見")]
        [StringLength(500)]
        public string? Comment { get; set; }

        public bool IsProxy { get; set; }
        public int ApproverRecordId { get; set; }
    }

    public class LeaveQueryViewModel
    {
        public string? ApplicantId { get; set; }
        public LeaveType? LeaveType { get; set; }
        public ApplicationStatus? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<LeaveApplication> Results { get; set; } = new List<LeaveApplication>();
    }
}
