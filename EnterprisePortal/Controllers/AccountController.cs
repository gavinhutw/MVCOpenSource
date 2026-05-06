using EnterprisePortal.Data;
using EnterprisePortal.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace EnterprisePortal.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AccountController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            GenerateCaptcha();
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var sessionCaptcha = HttpContext.Session.GetString("Captcha");

            if (string.IsNullOrEmpty(sessionCaptcha) ||
                !string.Equals(model.CaptchaInput, sessionCaptcha, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("CaptchaInput", "驗證碼不正確，請重新輸入");
                GenerateCaptcha();
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                GenerateCaptcha();
                return View(model);
            }

            var employee = await _db.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.EmployeeId == model.EmployeeId && e.EmploymentStatus == Models.EmploymentStatus.Active);

            if (employee == null || !VerifyPassword(model.Password, employee.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "員工編號或密碼不正確");
                GenerateCaptcha();
                return View(model);
            }

            // Load employee roles
            var roles = await _db.EmployeeRoles
                .Include(er => er.Role)
                .Where(er => er.EmployeeId == employee.EmployeeId)
                .Select(er => er.Role!.RoleName)
                .ToListAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, employee.EmployeeId),
                new Claim(ClaimTypes.Name, employee.FullName),
                new Claim("DepartmentId", employee.DepartmentId ?? ""),
                new Claim("DepartmentName", employee.Department?.DepartmentName ?? ""),
                new Claim("JobTitle", employee.JobTitle ?? "")
            };

            // Add role claims
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                new AuthenticationProperties { IsPersistent = false });

            HttpContext.Session.Remove("Captcha");
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult RefreshCaptcha()
        {
            GenerateCaptcha();
            var captcha = HttpContext.Session.GetString("Captcha") ?? "";
            return Json(new { captcha });
        }

        [HttpGet]
        public IActionResult CaptchaImage()
        {
            GenerateCaptcha();
            var captchaText = HttpContext.Session.GetString("Captcha") ?? "ERROR";
            var imageBytes = GenerateCaptchaImage(captchaText);
            return File(imageBytes, "image/png");
        }

        private void GenerateCaptcha()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var random = new Random();
            var captcha = new string(Enumerable.Repeat(chars, 5).Select(s => s[random.Next(s.Length)]).ToArray());
            HttpContext.Session.SetString("Captcha", captcha);
        }

        private static byte[] GenerateCaptchaImage(string text)
        {
            int width = 140, height = 40;
            var bitmap = new System.Drawing.Bitmap(width, height);
            using var g = System.Drawing.Graphics.FromImage(bitmap);

            g.Clear(System.Drawing.Color.WhiteSmoke);

            var random = new Random();
            // Draw noise lines
            for (int i = 0; i < 5; i++)
            {
                var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(random.Next(180, 220), random.Next(180, 220), random.Next(180, 220)));
                g.DrawLine(pen, random.Next(width), random.Next(height), random.Next(width), random.Next(height));
            }

            // Draw text
            var font = new System.Drawing.Font("Arial", 20, System.Drawing.FontStyle.Bold);
            var brush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(50, 80, 120));
            for (int i = 0; i < text.Length; i++)
            {
                float x = 10 + i * 24;
                float y = random.Next(2, 8);
                g.DrawString(text[i].ToString(), font, brush, x, y);
            }

            // Draw noise dots
            for (int i = 0; i < 80; i++)
            {
                var dotBrush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(random.Next(150, 200), random.Next(150, 200), random.Next(150, 200)));
                g.FillEllipse(dotBrush, random.Next(width), random.Next(height), 2, 2);
            }

            using var ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }

        private static bool VerifyPassword(string password, string hash)
        {
            // Simple SHA256 hash verification (use BCrypt in production)
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "EnterprisePortalSalt"));
            var computedHash = Convert.ToBase64String(hashBytes);
            return computedHash == hash;
        }

        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "EnterprisePortalSalt"));
            return Convert.ToBase64String(hashBytes);
        }
    }
}
