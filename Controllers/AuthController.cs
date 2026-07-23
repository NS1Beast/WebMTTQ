using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebMTTQ.Models;

namespace WebMTTQ.Controllers
{
    public class AuthController : Controller
    {
        private readonly DataMTTQContext _context;

        public AuthController(DataMTTQContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Nếu đã đăng nhập thì chuyển thẳng vào admin
            if (HttpContext.Session.GetString("AdminLoggedIn") == "true")
            {
                return RedirectToAction("Index", "Admin");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.NguoiDungs
                .FirstOrDefaultAsync(u => u.TenDangNhap == model.TenDangNhap
                                       && u.MatKhau == model.MatKhau
                                       && (u.DaXoa == null || u.DaXoa == false));

            if (user != null)
            {
                // Lưu thông tin vào session
                HttpContext.Session.SetString("AdminLoggedIn", "true");
                HttpContext.Session.SetString("AdminUserId", user.IdnguoiDung.ToString());
                HttpContext.Session.SetString("AdminHoTen", user.HoTen);
                HttpContext.Session.SetString("AdminTenDangNhap", user.TenDangNhap);

                return RedirectToAction("Index", "Admin");
            }

            ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}