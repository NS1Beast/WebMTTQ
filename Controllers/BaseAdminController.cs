using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebMTTQ.Services;

namespace WebMTTQ.Controllers
{
    /// <summary>
    /// Attribute để kiểm tra quyền truy cập theo module trên action.
    /// </summary>
    public class KiemTraQuyenAttribute : ActionFilterAttribute
    {
        private readonly string _maModule;
        private readonly string _hanhDong; // "Xem", "Them", "Sua", "Xoa"

        public KiemTraQuyenAttribute(string maModule, string hanhDong = "Xem")
        {
            _maModule = maModule;
            _hanhDong = hanhDong;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;

            // Nếu chưa đăng nhập thì chuyển hướng
            if (session.GetString("AdminLoggedIn") != "true")
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            bool hasPermission = _hanhDong switch
            {
                "Xem" => PhanQuyenHelper.CoQuyenXem(session, _maModule),
                "Them" => PhanQuyenHelper.CoQuyenThem(session, _maModule),
                "Sua" => PhanQuyenHelper.CoQuyenSua(session, _maModule),
                "Xoa" => PhanQuyenHelper.CoQuyenXoa(session, _maModule),
                _ => PhanQuyenHelper.CoQuyenXem(session, _maModule)
            };

            if (!hasPermission)
            {
                context.HttpContext.Session.SetString("ErrorMessage", "Bạn không có quyền thực hiện thao tác này!");
                context.Result = new RedirectToActionResult("Index", "Home", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }

    /// <summary>
    /// Base controller cho tất cả các trang admin.
    /// Tự động kiểm tra session đăng nhập trước mỗi action.
    /// </summary>
    public abstract class BaseAdminController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
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