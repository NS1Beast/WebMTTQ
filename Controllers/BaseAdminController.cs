using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebMTTQ.Models;
using WebMTTQ.Services;

namespace WebMTTQ.Controllers
{
    /// <summary>
    /// Attribute để kiểm tra quyền truy cập theo module và hành động.
    /// - Chưa đăng nhập → Redirect về trang Login.
    /// - Đã đăng nhập nhưng không có quyền → Trả về 403 Forbidden.
    /// 
    /// Nếu không chỉ định _hanhDong, sẽ tự động xác định dựa trên HTTP method:
    /// - GET → "Xem"
    /// - POST có action name chứa "Delete"/"Xoa" → "Xoa"
    /// - POST có action name chứa "Create"/"Them" → "Them"
    /// - POST khác → "Sua"
    /// </summary>
    public class KiemTraQuyenAttribute : ActionFilterAttribute
    {
        private readonly string _maModule;
        private readonly string? _hanhDongExplicit; // "Auto", "Xem", "Them", "Sua", "Xoa" nếu được chỉ định

        public KiemTraQuyenAttribute(string maModule, string hanhDong = "Auto")
        {
            _maModule = maModule;
            _hanhDongExplicit = hanhDong;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var session = context.HttpContext.Session;

            // Nếu chưa đăng nhập thì chuyển hướng về trang login
            if (session.GetString("AdminLoggedIn") != "true")
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            // Kiểm tra session permission có còn hợp lệ không.
            // Nếu role/permission đã thay đổi → reload permission mới từ database.
            await PhanQuyenHelper.RefreshSessionQuyenIfNeededAsync(context.HttpContext);

            // Xác định hành động cần kiểm tra
            string hanhDong = (_hanhDongExplicit == "Auto" || string.IsNullOrEmpty(_hanhDongExplicit))
                ? DetermineAction(context)
                : _hanhDongExplicit!;

            // Kiểm tra quyền theo module và hành động
            bool hasPermission = hanhDong switch
            {
                "Xem" => PhanQuyenHelper.CoQuyenXem(session, _maModule),
                "Them" => PhanQuyenHelper.CoQuyenThem(session, _maModule),
                "Sua" => PhanQuyenHelper.CoQuyenSua(session, _maModule),
                "Xoa" => PhanQuyenHelper.CoQuyenXoa(session, _maModule),
                _ => PhanQuyenHelper.CoQuyenXem(session, _maModule)
            };

            if (!hasPermission)
            {
                // Đã đăng nhập nhưng không có quyền → trả về 403 Access Denied
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
                return;
            }

            await next();
        }

        /// <summary>
        /// Tự động xác định hành động dựa trên HTTP method và tên action.
        /// </summary>
        private string DetermineAction(ActionExecutingContext context)
        {
            var method = context.HttpContext.Request.Method;
            var actionName = context.ActionDescriptor.RouteValues["action"]?.ToLower() ?? "";

            // Ưu tiên xác định từ tên action (bất kể GET hay POST)
            if (actionName.Contains("delete") || actionName.Contains("xoa"))
                return "Xoa";

            if (actionName.Contains("create") || actionName.Contains("them") || actionName.Contains("add") || actionName.Contains("import"))
                return "Them";

            if (actionName.Contains("edit") || actionName.Contains("sua") || actionName.Contains("update") || actionName.Contains("save"))
                return "Sua";

            // Nếu không xác định được từ tên action, dựa vào HTTP method
            // GET/HEAD/OPTIONS → Xem
            if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsOptions(method))
                return "Xem";

            // DELETE method → Xoa
            if (HttpMethods.IsDelete(method))
                return "Xoa";

            // POST/PUT/PATCH mặc định → Sua an toàn hơn Them
            if (HttpMethods.IsPost(method) || HttpMethods.IsPut(method) || HttpMethods.IsPatch(method))
                return "Sua";

            return "Xem";
        }
    }

    /// <summary>
    /// Base controller cho tất cả các trang admin.
    /// Tự động kiểm tra session đăng nhập trước mỗi action.
    /// </summary>
    public abstract class BaseAdminController : Controller
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Kiểm tra session
            if (HttpContext.Session.GetString("AdminLoggedIn") != "true")
            {
                // Chưa đăng nhập, chuyển hướng về trang login
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            // Kiểm tra session permission còn hợp lệ không.
            // Nếu role/permission đã thay đổi trong DB → reload permission mới.
            await PhanQuyenHelper.RefreshSessionQuyenIfNeededAsync(HttpContext);

            await next();
        }
    }
}