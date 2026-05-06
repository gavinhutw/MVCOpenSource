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
    [Authorize(Roles = "系統管理員")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public AdminController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // ==================== 員工管理 ====================

        public async Task<IActionResult> EmployeeList(string? search, string? deptId, EmploymentStatus? status)
        {
            var query = _db.Employees
                .Include(e => e.Department)
                .Include(e => e.Supervisor)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(e => e.EmployeeId.Contains(search) || e.FullName.Contains(search) || (e.EnglishName != null && e.EnglishName.Contains(search)));

            if (!string.IsNullOrEmpty(deptId))
                query = query.Where(e => e.DepartmentId == deptId);

            if (status.HasValue)
                query = query.Where(e => e.EmploymentStatus == status.Value);

            var employees = await query.OrderBy(e => e.EmployeeId).ToListAsync();

            ViewBag.DepartmentList = new SelectList(
                await _db.Departments.OrderBy(d => d.DepartmentName).ToListAsync(),
                "DepartmentId", "DepartmentName", deptId);
            ViewBag.Search = search;
            ViewBag.SelectedDept = deptId;
            ViewBag.SelectedStatus = status;

            return View(employees);
        }

        public async Task<IActionResult> EmployeeDetail(string id)
        {
            var emp = await _db.Employees
                .Include(e => e.Department)
                .Include(e => e.Supervisor)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);
            if (emp == null) return NotFound();

            var roles = await _db.EmployeeRoles
                .Include(er => er.Role)
                .Where(er => er.EmployeeId == id)
                .ToListAsync();
            ViewBag.Roles = roles;

            return View(emp);
        }

        [HttpGet]
        public async Task<IActionResult> EmployeeCreate()
        {
            await LoadFormSelects();
            return View(new Employee { HireDate = DateTime.Today, EmploymentStatus = EmploymentStatus.Active });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmployeeCreate(Employee model, IFormFile? photoFile, string? password)
        {
            if (await _db.Employees.AnyAsync(e => e.EmployeeId == model.EmployeeId))
                ModelState.AddModelError("EmployeeId", "此員工編號已存在");

            if (string.IsNullOrWhiteSpace(password))
                ModelState.AddModelError("", "新增員工時必須設定密碼");

            if (!ModelState.IsValid)
            {
                await LoadFormSelects(model);
                return View(model);
            }

            if (photoFile != null && photoFile.Length > 0)
                model.PhotoPath = await SavePhoto(model.EmployeeId, photoFile);

            model.PasswordHash = AccountController.HashPassword(password!);
            _db.Employees.Add(model);
            await _db.SaveChangesAsync();

            // Default EMPLOYEE role
            _db.EmployeeRoles.Add(new EmployeeRole { EmployeeId = model.EmployeeId, RoleId = "EMPLOYEE" });
            await _db.SaveChangesAsync();

            TempData["Success"] = $"員工 {model.FullName}（{model.EmployeeId}）已成功新增！";
            return RedirectToAction("EmployeeList");
        }

        [HttpGet]
        public async Task<IActionResult> EmployeeEdit(string id)
        {
            var emp = await _db.Employees.FindAsync(id);
            if (emp == null) return NotFound();
            await LoadFormSelects(emp);
            return View(emp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmployeeEdit(Employee model, IFormFile? photoFile, string? newPassword)
        {
            var existing = await _db.Employees.FindAsync(model.EmployeeId);
            if (existing == null) return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadFormSelects(model);
                return View(model);
            }

            if (photoFile != null && photoFile.Length > 0)
                existing.PhotoPath = await SavePhoto(model.EmployeeId, photoFile);

            existing.FullName = model.FullName;
            existing.EnglishName = model.EnglishName;
            existing.Gender = model.Gender;
            existing.IdNumber = model.IdNumber;
            existing.MaritalStatus = model.MaritalStatus;
            existing.EducationLevel = model.EducationLevel;
            existing.Address = model.Address;
            existing.Phone = model.Phone;
            existing.Mobile = model.Mobile;
            existing.Email = model.Email;
            existing.HireDate = model.HireDate;
            existing.EmploymentStatus = model.EmploymentStatus;
            existing.DepartmentId = model.DepartmentId;
            existing.SupervisorId = model.SupervisorId;
            existing.JobTitle = model.JobTitle;

            if (!string.IsNullOrWhiteSpace(newPassword))
                existing.PasswordHash = AccountController.HashPassword(newPassword);

            await _db.SaveChangesAsync();
            TempData["Success"] = "員工資料已更新！";
            return RedirectToAction("EmployeeList");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmployeeDelete(string id)
        {
            var emp = await _db.Employees.FindAsync(id);
            if (emp == null) return NotFound();

            // Soft delete: set to Resigned
            emp.EmploymentStatus = EmploymentStatus.Resigned;
            await _db.SaveChangesAsync();

            TempData["Success"] = $"員工 {emp.FullName}（{emp.EmployeeId}）已設為離職狀態。";
            return RedirectToAction("EmployeeList");
        }

        // ==================== 角色權限管理 ====================

        public async Task<IActionResult> RoleList()
        {
            var roles = await _db.Roles
                .Include(r => r.EmployeeRoles)
                .ToListAsync();
            return View(roles);
        }

        public async Task<IActionResult> RoleAssign(string id)
        {
            var emp = await _db.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);
            if (emp == null) return NotFound();

            var allRoles = await _db.Roles.ToListAsync();
            var empRoleIds = await _db.EmployeeRoles
                .Where(er => er.EmployeeId == id)
                .Select(er => er.RoleId)
                .ToListAsync();

            ViewBag.Employee = emp;
            ViewBag.AllRoles = allRoles;
            ViewBag.EmpRoleIds = empRoleIds;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RoleAssign(string employeeId, List<string> roleIds)
        {
            // Remove existing roles
            var existing = await _db.EmployeeRoles.Where(er => er.EmployeeId == employeeId).ToListAsync();
            _db.EmployeeRoles.RemoveRange(existing);

            // Add selected roles
            foreach (var roleId in roleIds.Where(r => !string.IsNullOrEmpty(r)))
            {
                _db.EmployeeRoles.Add(new EmployeeRole { EmployeeId = employeeId, RoleId = roleId });
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "角色指派已更新！";
            return RedirectToAction("EmployeeList");
        }

        [HttpGet]
        public async Task<IActionResult> RoleManage()
        {
            var roles = await _db.Roles
                .Include(r => r.EmployeeRoles)
                .ToListAsync();
            return View(roles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RoleCreate(Role model)
        {
            if (await _db.Roles.AnyAsync(r => r.RoleId == model.RoleId))
            {
                TempData["Error"] = "角色代碼已存在";
                return RedirectToAction("RoleManage");
            }
            _db.Roles.Add(model);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"角色「{model.RoleName}」已新增！";
            return RedirectToAction("RoleManage");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RoleDelete(string id)
        {
            var role = await _db.Roles.FindAsync(id);
            if (role == null) return NotFound();
            _db.Roles.Remove(role);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"角色「{role.RoleName}」已刪除。";
            return RedirectToAction("RoleManage");
        }

        // ==================== Helpers ====================

        private async Task<string> SavePhoto(string employeeId, IFormFile file)
        {
            var dir = Path.Combine(_env.WebRootPath, "uploads", "photos");
            Directory.CreateDirectory(dir);
            var fileName = $"{employeeId}{Path.GetExtension(file.FileName)}";
            using var stream = new FileStream(Path.Combine(dir, fileName), FileMode.Create);
            await file.CopyToAsync(stream);
            return $"/uploads/photos/{fileName}";
        }

        private async Task LoadFormSelects(Employee? emp = null)
        {
            ViewBag.DepartmentList = new SelectList(
                await _db.Departments.OrderBy(d => d.DepartmentName).ToListAsync(),
                "DepartmentId", "DepartmentName", emp?.DepartmentId);

            var supervisors = await _db.Employees
                .Where(e => e.EmploymentStatus == EmploymentStatus.Active && (emp == null || e.EmployeeId != emp.EmployeeId))
                .OrderBy(e => e.FullName)
                .Select(e => new { e.EmployeeId, DisplayName = e.EmployeeId + " " + e.FullName })
                .ToListAsync();
            ViewBag.SupervisorList = new SelectList(supervisors, "EmployeeId", "DisplayName", emp?.SupervisorId);

            ViewBag.GenderList = new SelectList(
                Enum.GetValues(typeof(GenderType)).Cast<GenderType>()
                    .Select(v => new SelectListItem { Value = ((int)v).ToString(), Text = v.GetDisplayName() }),
                "Value", "Text", emp == null ? null : (int)emp.Gender);

            ViewBag.MaritalStatusList = new SelectList(
                Enum.GetValues(typeof(MaritalStatus)).Cast<MaritalStatus>()
                    .Select(v => new SelectListItem { Value = ((int)v).ToString(), Text = v.GetDisplayName() }),
                "Value", "Text", emp == null ? null : (int)emp.MaritalStatus);

            ViewBag.EducationLevelList = new SelectList(
                Enum.GetValues(typeof(EducationLevel)).Cast<EducationLevel>()
                    .Select(v => new SelectListItem { Value = ((int)v).ToString(), Text = v.GetDisplayName() }),
                "Value", "Text", emp == null ? null : (int)emp.EducationLevel);

            ViewBag.EmploymentStatusList = new SelectList(
                Enum.GetValues(typeof(EmploymentStatus)).Cast<EmploymentStatus>()
                    .Select(v => new SelectListItem { Value = ((int)v).ToString(), Text = v.GetDisplayName() }),
                "Value", "Text", emp == null ? null : (int)emp.EmploymentStatus);
        }
    }
}
