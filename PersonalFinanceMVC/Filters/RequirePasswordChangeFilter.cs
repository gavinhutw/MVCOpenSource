using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PersonalFinanceMVC.Filters;

/// <summary>
/// 全域過濾器：若使用者的密碼已過期（claim PasswordExpired=True），
/// 強制導向修改密碼頁，Auth 與 Captcha Controller 除外。
/// </summary>
public class RequirePasswordChangeFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var user = context.HttpContext.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            var passwordExpired = user.FindFirst("PasswordExpired")?.Value == "True";

            if (passwordExpired)
            {
                var controller = context.RouteData.Values["controller"]?.ToString();

                // 允許 Auth（含 ChangePassword/Logout）和 Captcha
                if (controller is not ("Auth" or "Captcha"))
                {
                    context.Result = new RedirectToActionResult("ChangePassword", "Auth", null);
                    return;
                }
            }
        }

        await next();
    }
}
