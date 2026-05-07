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
    public class OvertimeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public OvertimeController(ApplicationDbContext db)
        {
            _db = db;
        }

        private string CurrentEmployeeId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        public async Task<IActionResult> Apply()
        {
            var vm = new OvertimeApplicationViewModel
            {
                EmployeeList = await GetEmployeeSelectList()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(OvertimeApplicationViewModel vm)
        {
            if (vm.EndDate < vm.StartDate)
                ModelState.AddModelError("EndDate", "結束日期不可早於起始日期");

            if (!vm.ApproverIds.Any())
                ModelState.AddModelError("ApproverIds", "請至少選擇一位簽核主管");

            if (!ModelState.IsValid)
            {
                vm.EmployeeList = await GetEmployeeSelectList();
                return View(vm);
            }

            var application = new OvertimeApplication
            {
                ApplicantId = CurrentEmployeeId,
                StartDate = vm.StartDate,
                EndDate = vm.EndDate,
                Reason = vm.Reason,
                ProxyId = null,
                Status = ApplicationStatus.Pending,
                AppliedAt = DateTime.Now
            };

            _db.OvertimeApplications.Add(application);
            await _db.SaveChangesAsync();

            for (int i = 0; i < vm.ApproverIds.Count; i++)
            {
                if (!string.IsNullOrEmpty(vm.ApproverIds[i]))
                {
                    _db.OvertimeApprovers.Add(new OvertimeApprover
                    {
                        OvertimeApplicationId = application.Id,
                        ApproverId = vm.ApproverIds[i],
                        ApprovalOrder = i,
                        Status = ApprovalStatus.Pending
                    });
                }
            }

            application.Status = ApplicationStatus.InProgress;
            await _db.SaveChangesAsync();

            TempData["Success"] = "加班申請已成功送出！";
            return RedirectToAction("Query");
        }

        public async Task<IActionResult> Approve()
        {
            var empId = CurrentEmployeeId;

            var pendingApprovals = await _db.OvertimeApprovers
                .Include(oa => oa.OvertimeApplication)
                    .ThenInclude(o => o!.Applicant)
                .Include(oa => oa.OvertimeApplication)
                    .ThenInclude(o => o!.Approvers)
                .Where(oa => oa.ApproverId == empId && oa.Status == ApprovalStatus.Pending)
                .OrderBy(oa => oa.OvertimeApplication!.AppliedAt)
                .ToListAsync();

            var eligibleApprovals = pendingApprovals.Where(oa =>
            {
                var prevApprovers = oa.OvertimeApplication!.Approvers
                    .Where(a => a.ApprovalOrder < oa.ApprovalOrder)
                    .ToList();
                return !prevApprovers.Any() || prevApprovers.All(a => a.Status == ApprovalStatus.Approved);
            }).ToList();

            ViewBag.PendingApprovals = eligibleApprovals;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessApproval(int approverRecordId, string action, string? comment)
        {
            var approverRecord = await _db.OvertimeApprovers
                .Include(oa => oa.OvertimeApplication)
                    .ThenInclude(o => o!.Approvers)
                .FirstOrDefaultAsync(oa => oa.Id == approverRecordId && oa.ApproverId == CurrentEmployeeId);

            if (approverRecord == null)
                return NotFound();

            approverRecord.Status = action == "approve" ? ApprovalStatus.Approved : ApprovalStatus.Rejected;
            approverRecord.Comment = comment;
            approverRecord.ApprovedAt = DateTime.Now;

            var application = approverRecord.OvertimeApplication!;

            if (action == "reject")
            {
                application.Status = ApplicationStatus.Rejected;
            }
            else
            {
                bool allApproved = application.Approvers.All(a => a.Status == ApprovalStatus.Approved);
                if (allApproved)
                    application.Status = ApplicationStatus.Approved;
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = action == "approve" ? "已同意此加班申請" : "已拒絕此加班申請";
            return RedirectToAction("Approve");
        }

        public async Task<IActionResult> Query(OvertimeQueryViewModel? vm)
        {
            vm ??= new OvertimeQueryViewModel();

            var query = _db.OvertimeApplications
                .Include(o => o.Applicant)
                .Include(o => o.Approvers).ThenInclude(a => a.Approver)
                .Where(o => o.ApplicantId == CurrentEmployeeId);

            if (vm.Status.HasValue)
                query = query.Where(o => o.Status == vm.Status.Value);

            if (vm.StartDate.HasValue)
                query = query.Where(o => o.StartDate >= vm.StartDate.Value);

            if (vm.EndDate.HasValue)
                query = query.Where(o => o.EndDate <= vm.EndDate.Value);

            vm.Results = await query.OrderByDescending(o => o.AppliedAt).ToListAsync();
            return View(vm);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var overtime = await _db.OvertimeApplications
                .Include(o => o.Applicant)
                .Include(o => o.Proxy)
                .Include(o => o.Approvers).ThenInclude(a => a.Approver)
                .FirstOrDefaultAsync(o => o.Id == id && o.ApplicantId == CurrentEmployeeId);

            if (overtime == null) return NotFound();
            return View(overtime);
        }

        private async Task<SelectList> GetEmployeeSelectList()
        {
            var employees = await _db.Employees
                .Where(e => e.EmploymentStatus == EmploymentStatus.Active && e.EmployeeId != CurrentEmployeeId)
                .OrderBy(e => e.FullName)
                .Select(e => new { e.EmployeeId, DisplayName = e.EmployeeId + " " + e.FullName })
                .ToListAsync();
            return new SelectList(employees, "EmployeeId", "DisplayName");
        }
    }
}
