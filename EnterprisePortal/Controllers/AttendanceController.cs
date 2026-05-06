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
    public class AttendanceController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AttendanceController(ApplicationDbContext db)
        {
            _db = db;
        }

        private string CurrentEmployeeId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        public IActionResult Index()
        {
            var vm = new AttendanceViewModel
            {
                ClockTypeList = new SelectList(
                    Enum.GetValues(typeof(ClockType)).Cast<ClockType>()
                        .Select(v => new SelectListItem { Value = ((int)v).ToString(), Text = v.GetDisplayName() }),
                    "Value", "Text")
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(AttendanceViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.ClockTypeList = new SelectList(
                    Enum.GetValues(typeof(ClockType)).Cast<ClockType>()
                        .Select(v => new SelectListItem { Value = ((int)v).ToString(), Text = v.GetDisplayName() }),
                    "Value", "Text");
                return View(vm);
            }

            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "未知";

            var record = new AttendanceRecord
            {
                EmployeeId = CurrentEmployeeId,
                ClockType = vm.ClockType,
                Temperature = vm.Temperature,
                RecordedAt = DateTime.Now,
                IpAddress = clientIp
            };

            _db.AttendanceRecords.Add(record);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"{vm.ClockType.GetDisplayName()} 刷卡成功！時間：{record.RecordedAt:yyyy/MM/dd HH:mm:ss}";
            return RedirectToAction("Index");
        }
    }
}
