#pragma warning disable CA1416  // Windows 平台 API（System.Drawing.Common）

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.Drawing.Imaging;

namespace PersonalFinanceMVC.Controllers;

[AllowAnonymous]
[Route("[controller]/[action]")]
public class CaptchaController : Controller
{
    private const string SessionKey = "CaptchaCode";
    private const string Chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    [HttpGet]
    public IActionResult Image()
    {
        // 產生 5 碼驗證碼（排除容易混淆的 0/O/1/I）
        var code = new string(Enumerable.Range(0, 5)
            .Select(_ => Chars[Random.Shared.Next(Chars.Length)]).ToArray());

        HttpContext.Session.SetString(SessionKey, code.ToUpper());

        const int Width = 140, Height = 44;
        using var bmp = new Bitmap(Width, Height);
        using var g = Graphics.FromImage(bmp);

        // 背景
        g.Clear(Color.FromArgb(235, 240, 255));

        // 干擾線
        for (int i = 0; i < 5; i++)
        {
            using var pen = new Pen(Color.FromArgb(
                Random.Shared.Next(150, 210),
                Random.Shared.Next(150, 210),
                Random.Shared.Next(150, 210)), 1);
            g.DrawLine(pen,
                Random.Shared.Next(Width), Random.Shared.Next(Height),
                Random.Shared.Next(Width), Random.Shared.Next(Height));
        }

        // 每個字元隨機顏色 + 輕微旋轉
        var charColors = new[]
        {
            Color.FromArgb(30, 80, 160),
            Color.FromArgb(160, 30, 60),
            Color.FromArgb(20, 120, 60),
            Color.FromArgb(120, 60, 160),
            Color.FromArgb(160, 100, 20)
        };

        for (int i = 0; i < code.Length; i++)
        {
            float angle = Random.Shared.Next(-18, 18);
            float x = 10 + i * 25;
            float y = 8 + Random.Shared.Next(-3, 3);

            g.TranslateTransform(x + 8, y + 10);
            g.RotateTransform(angle);
            using var font = new Font("Arial", 19, FontStyle.Bold);
            using var brush = new SolidBrush(charColors[i % charColors.Length]);
            g.DrawString(code[i].ToString(), font, brush, -8, -10);
            g.RotateTransform(-angle);
            g.TranslateTransform(-(x + 8), -(y + 10));
        }

        // 干擾點
        for (int i = 0; i < 120; i++)
        {
            bmp.SetPixel(Random.Shared.Next(Width), Random.Shared.Next(Height),
                Color.FromArgb(
                    Random.Shared.Next(80, 200),
                    Random.Shared.Next(80, 200),
                    Random.Shared.Next(80, 200)));
        }

        using var ms = new MemoryStream();
        bmp.Save(ms, ImageFormat.Png);
        return File(ms.ToArray(), "image/png");
    }

    /// <summary>供 AuthController 驗證用</summary>
    public static bool Validate(ISession session, string input) =>
        string.Equals(session.GetString(SessionKey), input?.Trim().ToUpper(),
                      StringComparison.OrdinalIgnoreCase);

    /// <summary>驗證後清除 Session，防止重複使用</summary>
    public static void Clear(ISession session) =>
        session.Remove(SessionKey);
}
