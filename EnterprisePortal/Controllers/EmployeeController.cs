using EnterprisePortal.Data;
using EnterprisePortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using EnterprisePortal.Controllers;

namespace EnterprisePortal.Controllers
{
    [Authorize]
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public EmployeeController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        private string CurrentEmployeeId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string? id)
        {
            if (string.IsNullOrEmpty(id))
                id = CurrentEmployeeId;

            var employee = await _db.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null)
            {
                TempData["Error"] = $"找不到員工編號：{id}";
                return View("Index");
            }

            await LoadSelectLists(employee);
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Employee model, IFormFile? photoFile, string? newPassword)
        {
            // Only allow editing own record unless admin
            if (model.EmployeeId != CurrentEmployeeId)
            {
                TempData["Error"] = "您只能維護自己的資料";
                return RedirectToAction("Index");
            }

            var existing = await _db.Employees.FindAsync(model.EmployeeId);
            if (existing == null) return NotFound();

            // Handle photo upload
            if (photoFile != null && photoFile.Length > 0)
            {
                var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "photos");
                Directory.CreateDirectory(uploadDir);
                var fileName = $"{model.EmployeeId}{Path.GetExtension(photoFile.FileName)}";
                var filePath = Path.Combine(uploadDir, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await photoFile.CopyToAsync(stream);
                existing.PhotoPath = $"/uploads/photos/{fileName}";
            }

            // Update fields
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
            TempData["Success"] = "員工資料已成功更新！";
            return RedirectToAction("Edit", new { id = model.EmployeeId });
        }

        private async Task LoadSelectLists(Employee employee)
        {
            var departments = await _db.Departments.OrderBy(d => d.DepartmentName).ToListAsync();
            ViewBag.DepartmentList = new SelectList(departments, "DepartmentId", "DepartmentName", employee.DepartmentId);

            var supervisors = await _db.Employees
                .Where(e => e.EmploymentStatus == EmploymentStatus.Active && e.EmployeeId != employee.EmployeeId)
                .OrderBy(e => e.FullName)
                .Select(e => new { e.EmployeeId, DisplayName = e.EmployeeId + " " + e.FullName })
                .ToListAsync();
            ViewBag.SupervisorList = new SelectList(supervisors, "EmployeeId", "DisplayName", employee.SupervisorId);

            ViewBag.GenderList = new SelectList(
                Enum.GetValues(typeof(GenderType)).Cast<GenderType>()
                    .Select(v => new SelectListItem { Value = ((int)v).ToString(), Text = v.GetDisplayName() }),
                "Value", "Text", (int)employee.Gender);

            ViewBag.MaritalStatusList = new SelectList(
                Enum.GetValues(typeof(MaritalStatus)).Cast<MaritalStatus>()
                    .Select(v => new SelectListItem { Value = ((int)v).ToString(), Text = v.GetDisplayName() }),
                "Value", "Text", (int)employee.MaritalStatus);

            ViewBag.EducationLevelList = new SelectList(
                Enum.GetValues(typeof(EducationLevel)).Cast<EducationLevel>()
                    .Select(v => new SelectListItem { Value = ((int)v).ToString(), Text = v.GetDisplayName() }),
                "Value", "Text", (int)employee.EducationLevel);

            ViewBag.EmploymentStatusList = new SelectList(
                Enum.GetValues(typeof(EmploymentStatus)).Cast<EmploymentStatus>()
                    .Select(v => new SelectListItem { Value = ((int)v).ToString(), Text = v.GetDisplayName() }),
                "Value", "Text", (int)employee.EmploymentStatus);
        }
    }
}
