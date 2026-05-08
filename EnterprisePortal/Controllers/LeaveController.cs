using EnterprisePortal.Data;
using EnterprisePortal.Models;
using EnterprisePortal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EnterprisePortal.Controllers
{
    [Authorize]
    public class LeaveController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public LeaveController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        private string CurrentEmployeeId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        // 請假申請
        public async Task<IActionResult> Apply()
        {
            var employees = await _db.Employees
                .Where(e => e.EmploymentStatus == EmploymentStatus.Active && e.EmployeeId != CurrentEmployeeId)
                .OrderBy(e => e.FullName)
                .Select(e => new { e.EmployeeId, DisplayName = e.EmployeeId + " " + e.FullName })
                .ToListAsync();

            var vm = new LeaveApplicationViewModel
            {
                LeaveTypeList = new SelectList(Enum.GetValues(typeof(LeaveType)).Cast<LeaveType>()
                    .Select(v => new SelectListItem { Value = ((int)v).ToString(), Text = v.GetDisplayName() }), "Value", "Text"),
                EmployeeList = new SelectList(employees, "EmployeeId", "DisplayName")
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(LeaveApplicationViewModel vm)
        {
            if (vm.EndDate < vm.StartDate)
                ModelState.AddModelError("EndDate", "結束日期不可早於起始日期");

            if (!vm.ApproverIds.Any())
                ModelState.AddModelError("ApproverIds", "請至少選擇一位簽核主管");

            if (!ModelState.IsValid)
            {
                await ReloadEmployeeList(vm);
                return View(vm);
            }

            string? attachmentPath = null;
            if (vm.Attachment != null && vm.Attachment.Length > 0)
            {
                var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "attachments");
                Directory.CreateDirectory(uploadDir);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(vm.Attachment.FileName)}";
                var filePath = Path.Combine(uploadDir, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await vm.Attachment.CopyToAsync(stream);
                attachmentPath = $"/uploads/attachments/{fileName}";
            }

            var application = new LeaveApplication
            {
                ApplicantId = CurrentEmployeeId,
                LeaveType = vm.LeaveType,
                StartDate = vm.StartDate,
                StartHour = vm.StartHour,
                EndDate = vm.EndDate,
                EndHour = vm.EndHour,
                Reason = vm.Reason,
                ProxyId = vm.ProxyId,
                AttachmentPath = attachmentPath,
                Status = ApplicationStatus.Pending,
                AppliedAt = DateTime.Now
            };

            _db.LeaveApplications.Add(application);
            await _db.SaveChangesAsync();

            // Add proxy as first approver (order 0)
            _db.LeaveApprovers.Add(new LeaveApprover
            {
                LeaveApplicationId = application.Id,
                ApproverId = vm.ProxyId,
                ApprovalOrder = 0,
                Status = ApprovalStatus.Pending
            });

            // Add supervisors in order (starting from 1)
            for (int i = 0; i < vm.ApproverIds.Count; i++)
            {
                if (!string.IsNullOrEmpty(vm.ApproverIds[i]))
                {
                    _db.LeaveApprovers.Add(new LeaveApprover
                    {
                        LeaveApplicationId = application.Id,
                        ApproverId = vm.ApproverIds[i],
                        ApprovalOrder = i + 1,
                        Status = ApprovalStatus.Pending
                    });
                }
            }

            application.Status = ApplicationStatus.InProgress;
            await _db.SaveChangesAsync();

            TempData["Success"] = "請假申請已成功送出！";
            return RedirectToAction("Query");
        }

        // 請假批示
        public async Task<IActionResult> Approve()
        {
            var empId = CurrentEmployeeId;

            // Get applications where this employee is the current approver
            var pendingApprovals = await _db.LeaveApprovers
                .Include(la => la.LeaveApplication)
                    .ThenInclude(l => l!.Applicant)
                .Where(la => la.ApproverId == empId && la.Status == ApprovalStatus.Pending)
                .OrderBy(la => la.LeaveApplication!.AppliedAt)
                .ToListAsync();

            // Filter: only show if previous approver has approved (or it's the first)
            var eligibleApprovals = pendingApprovals.Where(la =>
            {
                var prevApprovers = la.LeaveApplication!.Approvers
                    .Where(a => a.ApprovalOrder < la.ApprovalOrder)
                    .ToList();
                return !prevApprovers.Any() || prevApprovers.All(a => a.Status == ApprovalStatus.Approved);
            }).ToList();

            // Reload with full approvers for each application
            var applicationIds = eligibleApprovals.Select(a => a.LeaveApplicationId).Distinct().ToList();
            var fullApplications = await _db.LeaveApplications
                .Include(l => l.Applicant)
                .Include(l => l.Approvers)
                .Where(l => applicationIds.Contains(l.Id))
                .ToListAsync();

            ViewBag.PendingApprovals = eligibleApprovals;
            ViewBag.FullApplications = fullApplications;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessApproval(int approverRecordId, string action, string? comment)
        {
            var approverRecord = await _db.LeaveApprovers
                .Include(la => la.LeaveApplication)
                    .ThenInclude(l => l!.Approvers)
                .FirstOrDefaultAsync(la => la.Id == approverRecordId && la.ApproverId == CurrentEmployeeId);

            if (approverRecord == null)
                return NotFound();

            approverRecord.Status = action == "approve" ? ApprovalStatus.Approved : ApprovalStatus.Rejected;
            approverRecord.Comment = comment;
            approverRecord.ApprovedAt = DateTime.Now;

            var application = approverRecord.LeaveApplication!;

            if (action == "reject")
            {
                application.Status = ApplicationStatus.Rejected;
            }
            else
            {
                // Check if all approvers have approved
                var allApprovers = application.Approvers.ToList();
                bool allApproved = allApprovers.All(a => a.Status == ApprovalStatus.Approved);
                if (allApproved)
                    application.Status = ApplicationStatus.Approved;
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = action == "approve" ? "已同意此假單" : "已拒絕此假單";
            return RedirectToAction("Approve");
        }

        // 請假記錄查詢
        public async Task<IActionResult> Query(LeaveQueryViewModel? vm)
        {
            vm ??= new LeaveQueryViewModel();

            var query = _db.LeaveApplications
                .Include(l => l.Applicant)
                .Include(l => l.Approvers).ThenInclude(a => a.Approver)
                .Where(l => l.ApplicantId == CurrentEmployeeId);

            if (vm.LeaveType.HasValue)
                query = query.Where(l => l.LeaveType == vm.LeaveType.Value);

            if (vm.Status.HasValue)
                query = query.Where(l => l.Status == vm.Status.Value);

            if (vm.StartDate.HasValue)
                query = query.Where(l => l.StartDate >= vm.StartDate.Value);

            if (vm.EndDate.HasValue)
                query = query.Where(l => l.EndDate <= vm.EndDate.Value);

            vm.Results = await query.OrderByDescending(l => l.AppliedAt).ToListAsync();
            return View(vm);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var leave = await _db.LeaveApplications
                .Include(l => l.Applicant)
                .Include(l => l.Proxy)
                .Include(l => l.Approvers).ThenInclude(a => a.Approver)
                .FirstOrDefaultAsync(l => l.Id == id && l.ApplicantId == CurrentEmployeeId);

            if (leave == null) return NotFound();
            return View(leave);
        }

        private async Task ReloadEmployeeList(LeaveApplicationViewModel vm)
        {
            var employees = await _db.Employees
                .Where(e => e.EmploymentStatus == EmploymentStatus.Active && e.EmployeeId != CurrentEmployeeId)
                .OrderBy(e => e.FullName)
                .Select(e => new { e.EmployeeId, DisplayName = e.EmployeeId + " " + e.FullName })
                .ToListAsync();

            vm.LeaveTypeList = new SelectList(Enum.GetValues(typeof(LeaveType)).Cast<LeaveType>()
                .Select(v => new SelectListItem { Value = ((int)v).ToString(), Text = v.GetDisplayName() }), "Value", "Text");
            vm.EmployeeList = new SelectList(employees, "EmployeeId", "DisplayName");
        }
    }
}
