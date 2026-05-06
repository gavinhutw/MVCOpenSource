using EnterprisePortal.Data;
using EnterprisePortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EnterprisePortal.Controllers
{
    [Authorize(Roles = "系統管理員,主管")]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ReportController(ApplicationDbContext db)
        {
            _db = db;
        }

        private string CurrentEmployeeId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        private bool IsAdmin => User.IsInRole("系統管理員");

        // ==================== 請假統計 ====================
        public async Task<IActionResult> LeaveReport(string? deptId, int? year, int? month, LeaveType? leaveType)
        {
            year ??= DateTime.Today.Year;

            var query = _db.LeaveApplications
                .Include(l => l.Applicant).ThenInclude(e => e!.Department)
                .Where(l => l.Status == ApplicationStatus.Approved)
                .Where(l => l.StartDate.Year == year.Value)
                .AsQueryable();

            if (!IsAdmin)
                query = query.Where(l => l.Applicant!.DepartmentId ==
                    _db.Employees.Where(e => e.EmployeeId == CurrentEmployeeId)
                        .Select(e => e.DepartmentId).FirstOrDefault());

            if (!string.IsNullOrEmpty(deptId))
                query = query.Where(l => l.Applicant!.DepartmentId == deptId);

            if (month.HasValue)
                query = query.Where(l => l.StartDate.Month == month.Value);

            if (leaveType.HasValue)
                query = query.Where(l => l.LeaveType == leaveType.Value);

            var leaves = await query.OrderBy(l => l.Applicant!.DepartmentId).ThenBy(l => l.ApplicantId).ToListAsync();

            // Stats by type
            var byType = leaves.GroupBy(l => l.LeaveType)
                .Select(g => new { Type = g.Key.GetDisplayName(), Count = g.Count(), Days = g.Sum(l => (l.EndDate - l.StartDate).Days + 1) })
                .OrderByDescending(x => x.Count)
                .ToList();

            // Stats by department
            var byDept = leaves.GroupBy(l => l.Applicant?.Department?.DepartmentName ?? "未分配")
                .Select(g => new { Dept = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            // Stats by month
            var byMonth = Enumerable.Range(1, 12)
                .Select(m => new { Month = m, Count = leaves.Count(l => l.StartDate.Month == m) })
                .ToList();

            ViewBag.ByType = byType;
            ViewBag.ByDept = byDept;
            ViewBag.ByMonth = byMonth;
            ViewBag.TotalLeaves = leaves.Count;
            ViewBag.TotalDays = leaves.Sum(l => (l.EndDate - l.StartDate).Days + 1);
            ViewBag.Year = year;
            ViewBag.Month = month;
            ViewBag.SelectedDept = deptId;
            ViewBag.SelectedType = leaveType;
            ViewBag.DepartmentList = new SelectList(await _db.Departments.OrderBy(d => d.DepartmentName).ToListAsync(), "DepartmentId", "DepartmentName", deptId);
            ViewBag.YearList = Enumerable.Range(DateTime.Today.Year - 3, 5).Reverse().Select(y => new SelectListItem { Value = y.ToString(), Text = y + "年", Selected = y == year }).ToList();

            return View(leaves);
        }

        // ==================== 加班統計 ====================
        public async Task<IActionResult> OvertimeReport(string? deptId, int? year, int? month)
        {
            year ??= DateTime.Today.Year;

            var query = _db.OvertimeApplications
                .Include(o => o.Applicant).ThenInclude(e => e!.Department)
                .Where(o => o.Status == ApplicationStatus.Approved)
                .Where(o => o.StartDate.Year == year.Value)
                .AsQueryable();

            if (!IsAdmin)
                query = query.Where(o => o.Applicant!.DepartmentId ==
                    _db.Employees.Where(e => e.EmployeeId == CurrentEmployeeId)
                        .Select(e => e.DepartmentId).FirstOrDefault());

            if (!string.IsNullOrEmpty(deptId))
                query = query.Where(o => o.Applicant!.DepartmentId == deptId);

            if (month.HasValue)
                query = query.Where(o => o.StartDate.Month == month.Value);

            var overtimes = await query.OrderBy(o => o.Applicant!.DepartmentId).ThenBy(o => o.ApplicantId).ToListAsync();

            var byDept = overtimes.GroupBy(o => o.Applicant?.Department?.DepartmentName ?? "未分配")
                .Select(g => new { Dept = g.Key, Count = g.Count(), Days = g.Sum(o => (o.EndDate - o.StartDate).Days + 1) })
                .OrderByDescending(x => x.Days)
                .ToList();

            var byMonth = Enumerable.Range(1, 12)
                .Select(m => new { Month = m, Count = overtimes.Count(o => o.StartDate.Month == m) })
                .ToList();

            ViewBag.ByDept = byDept;
            ViewBag.ByMonth = byMonth;
            ViewBag.Total = overtimes.Count;
            ViewBag.TotalDays = overtimes.Sum(o => (o.EndDate - o.StartDate).Days + 1);
            ViewBag.Year = year;
            ViewBag.Month = month;
            ViewBag.SelectedDept = deptId;
            ViewBag.DepartmentList = new SelectList(await _db.Departments.OrderBy(d => d.DepartmentName).ToListAsync(), "DepartmentId", "DepartmentName", deptId);
            ViewBag.YearList = Enumerable.Range(DateTime.Today.Year - 3, 5).Reverse().Select(y => new SelectListItem { Value = y.ToString(), Text = y + "年", Selected = y == year }).ToList();

            return View(overtimes);
        }

        // ==================== 刷卡統計 ====================
        public async Task<IActionResult> AttendanceReport(string? empId, int? year, int? month)
        {
            year ??= DateTime.Today.Year;
            month ??= DateTime.Today.Month;

            var query = _db.AttendanceRecords
                .Include(a => a.Employee).ThenInclude(e => e!.Department)
                .Where(a => a.RecordedAt.Year == year.Value && a.RecordedAt.Month == month.Value)
                .AsQueryable();

            if (!IsAdmin)
                query = query.Where(a => a.EmployeeId == CurrentEmployeeId);
            else if (!string.IsNullOrEmpty(empId))
                query = query.Where(a => a.EmployeeId == empId);

            var records = await query.OrderBy(a => a.EmployeeId).ThenBy(a => a.RecordedAt).ToListAsync();

            // High temperature alerts (>37.5)
            var alerts = records.Where(a => a.Temperature > 37.5m).ToList();

            // Daily attendance summary
            var dailySummary = records
                .GroupBy(a => new { a.EmployeeId, Date = a.RecordedAt.Date })
                .Select(g => new
                {
                    g.Key.EmployeeId,
                    EmployeeName = g.First().Employee?.FullName,
                    g.Key.Date,
                    ClockIn = g.Where(a => a.ClockType == ClockType.ClockIn).OrderBy(a => a.RecordedAt).FirstOrDefault()?.RecordedAt,
                    ClockOut = g.Where(a => a.ClockType == ClockType.ClockOut).OrderByDescending(a => a.RecordedAt).FirstOrDefault()?.RecordedAt,
                    MaxTemp = g.Max(a => a.Temperature)
                })
                .OrderBy(x => x.Date).ThenBy(x => x.EmployeeId)
                .ToList();

            ViewBag.Records = records;
            ViewBag.Alerts = alerts;
            ViewBag.DailySummary = dailySummary;
            ViewBag.Year = year;
            ViewBag.Month = month;
            ViewBag.EmpId = empId;
            ViewBag.YearList = Enumerable.Range(DateTime.Today.Year - 2, 4).Reverse().Select(y => new SelectListItem { Value = y.ToString(), Text = y + "年", Selected = y == year }).ToList();
            ViewBag.MonthList = Enumerable.Range(1, 12).Select(m => new SelectListItem { Value = m.ToString(), Text = m + "月", Selected = m == month }).ToList();
            ViewBag.EmployeeList = new SelectList(
                await _db.Employees.Where(e => e.EmploymentStatus == EmploymentStatus.Active)
                    .OrderBy(e => e.FullName).Select(e => new { e.EmployeeId, Name = e.EmployeeId + " " + e.FullName }).ToListAsync(),
                "EmployeeId", "Name", empId);

            return View(records);
        }

        // ==================== 綜合儀表板 ====================
        [Authorize(Roles = "系統管理員")]
        public async Task<IActionResult> Dashboard()
        {
            var today = DateTime.Today;
            var thisMonth = new DateTime(today.Year, today.Month, 1);

            ViewBag.TotalEmployees = await _db.Employees.CountAsync(e => e.EmploymentStatus == EmploymentStatus.Active);
            ViewBag.TotalDepts = await _db.Departments.CountAsync();

            // Pending approvals
            ViewBag.PendingLeaves = await _db.LeaveApplications.CountAsync(l => l.Status == ApplicationStatus.InProgress);
            ViewBag.PendingOvertimes = await _db.OvertimeApplications.CountAsync(o => o.Status == ApplicationStatus.InProgress);

            // This month stats
            ViewBag.MonthLeaves = await _db.LeaveApplications
                .CountAsync(l => l.Status == ApplicationStatus.Approved && l.AppliedAt >= thisMonth);
            ViewBag.MonthOvertimes = await _db.OvertimeApplications
                .CountAsync(o => o.Status == ApplicationStatus.Approved && o.AppliedAt >= thisMonth);
            ViewBag.TodayClockIns = await _db.AttendanceRecords
                .CountAsync(a => a.ClockType == ClockType.ClockIn && a.RecordedAt.Date == today);

            // High temp today
            ViewBag.HighTempToday = await _db.AttendanceRecords
                .CountAsync(a => a.RecordedAt.Date == today && a.Temperature > 37.5m);

            // Monthly leave chart (last 6 months)
            var sixMonthAgo = today.AddMonths(-5);
            var leaveByMonth = await _db.LeaveApplications
                .Where(l => l.Status == ApplicationStatus.Approved && l.AppliedAt >= new DateTime(sixMonthAgo.Year, sixMonthAgo.Month, 1))
                .GroupBy(l => new { l.AppliedAt.Year, l.AppliedAt.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .ToListAsync();

            // Leave by type (this year)
            var leaveByType = await _db.LeaveApplications
                .Where(l => l.Status == ApplicationStatus.Approved && l.AppliedAt.Year == today.Year)
                .GroupBy(l => l.LeaveType)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToListAsync();

            // Dept employee count
            var deptCount = await _db.Employees
                .Where(e => e.EmploymentStatus == EmploymentStatus.Active && e.DepartmentId != null)
                .Include(e => e.Department)
                .GroupBy(e => e.Department!.DepartmentName)
                .Select(g => new { Dept = g.Key, Count = g.Count() })
                .ToListAsync();

            ViewBag.LeaveByMonth = leaveByMonth;
            ViewBag.LeaveByType = leaveByType;
            ViewBag.DeptCount = deptCount;

            return View();
        }
    }
}
