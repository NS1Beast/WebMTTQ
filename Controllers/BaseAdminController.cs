using Microsoft.AspNetCore.Mvc;

namespace WebMTTQ.Controllers
{
    /// <summary>
    /// Base controller cho tất cả các trang admin.
    /// Tự động kiểm tra session đăng nhập trước mỗi action.
    /// </summary>
    public abstract class BaseAdminController : Controller
    {
        public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            // Kiểm tra session
            if (HttpContext.Session.GetString("AdminLoggedIn") != "true")
            {
                // Chưa đăng nhập, chuyển hướng về trang login
                context.Result = new RedirectToActionResult("Login", "Auth", null);
            }
            base.OnActionExecuting(context);
        }
    }
}