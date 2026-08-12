using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebMTTQ.Models;
using WebMTTQ.Services;
using System.Threading.Tasks;

namespace WebMTTQ.Controllers
{
    [Route("TaiKhoan")]
    public class TaiKhoanController : BaseAdminController
    {
        private readonly DataMTTQContext _context;
        private readonly IQuyenTruyCapService _quyenService;

        public TaiKhoanController(DataMTTQContext context, IQuyenTruyCapService quyenService)
        {
            _context = context;
            _quyenService = quyenService;
        }

        // ================================================
        // TRANG CÁ NHÂN
        // ================================================

        [Route("")]
        [Route("ThongTin")]
        [HttpGet]
        public async Task<IActionResult> ThongTin()
        {
            var userId = int.Parse(HttpContext.Session.GetString("AdminUserId") ?? "0");
            if (userId <= 0) return RedirectToAction("Login", "Auth");

            var user = await _context.NguoiDungs
                .Include(u => u.IdvaiTroNavigation)
                .FirstOrDefaultAsync(u => u.IdnguoiDung == userId);

            if (user == null) return NotFound();

            var modules = await _quyenService.GetModulesDuocQuyenAsync(userId);
            var moduleNames = ModuleQuyen.GetAllModules()
                .Where(m => modules.Contains(m.MaModule))
                .Select(m => m.TenModule)
                .ToList();

            var model = new ThongTinCaNhanViewModel
            {
                IdnguoiDung = user.IdnguoiDung,
                TenDangNhap = user.TenDangNhap,
                HoTen = user.HoTen,
                Email = user.Email,
                SoDienThoai = user.SoDienThoai,
                TenVaiTro = user.IdvaiTroNavigation?.TenVaiTro,
                NgayTao = user.NgayTao,
                NgayCapNhat = user.NgayCapNhat,
                AnhDaiDien = user.AnhDaiDien,
                ModulesDuocQuyen = moduleNames
            };

            return View("~/Views/TaiKhoan/ThongTin.cshtml", model);
        }

        [Route("ThongTin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThongTin(ThongTinCaNhanViewModel model)
        {
            var userId = int.Parse(HttpContext.Session.GetString("AdminUserId") ?? "0");
            if (userId <= 0) return RedirectToAction("Login", "Auth");

            var user = await _context.NguoiDungs
                .Include(u => u.IdvaiTroNavigation)
                .FirstOrDefaultAsync(u => u.IdnguoiDung == userId);

            if (user == null) return NotFound();

            // Không cho đổi tên đăng nhập
            model.TenDangNhap = user.TenDangNhap;
            model.IdnguoiDung = user.IdnguoiDung;

            // Kiểm tra email đã tồn tại ở người khác chưa
            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                var emailExists = await _context.NguoiDungs
                    .AnyAsync(u => u.Email == model.Email && u.IdnguoiDung != userId);
                if (emailExists)
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng bởi tài khoản khác.");
                }
            }

            if (ModelState.IsValid)
            {
                user.HoTen = model.HoTen;
                user.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
                user.SoDienThoai = string.IsNullOrWhiteSpace(model.SoDienThoai) ? null : model.SoDienThoai.Trim();
                user.NgayCapNhat = DateTime.Now;

                await _context.SaveChangesAsync();

                // Cập nhật session
                HttpContext.Session.SetString("AdminHoTen", user.HoTen);

                TempData["SuccessMessage"] = "Đã cập nhật thông tin cá nhân thành công!";
                return RedirectToAction(nameof(ThongTin));
            }

            // Load lại thông tin
            var modules = await _quyenService.GetModulesDuocQuyenAsync(userId);
            var moduleNames = ModuleQuyen.GetAllModules()
                .Where(m => modules.Contains(m.MaModule))
                .Select(m => m.TenModule)
                .ToList();

            model.TenVaiTro = user.IdvaiTroNavigation?.TenVaiTro;
            model.NgayTao = user.NgayTao;
            model.NgayCapNhat = user.NgayCapNhat;
            model.AnhDaiDien = user.AnhDaiDien;
            model.ModulesDuocQuyen = moduleNames;

            return View("~/Views/TaiKhoan/ThongTin.cshtml", model);
        }

        // ================================================
        // ĐỔI MẬT KHẨU
        // ================================================

        [Route("DoiMatKhau")]
        [HttpGet]
        public IActionResult DoiMatKhau()
        {
            return View("~/Views/TaiKhoan/DoiMatKhau.cshtml", new DoiMatKhauViewModel());
        }

        [Route("DoiMatKhau")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiMatKhau(DoiMatKhauViewModel model)
        {
            var userId = int.Parse(HttpContext.Session.GetString("AdminUserId") ?? "0");
            if (userId <= 0) return RedirectToAction("Login", "Auth");

            var user = await _context.NguoiDungs.FindAsync(userId);
            if (user == null) return NotFound();

            // Kiểm tra mật khẩu hiện tại
            if (!PasswordHelper.VerifyPassword(model.MatKhauHienTai, user.MatKhau))
            {
                ModelState.AddModelError("MatKhauHienTai", "Mật khẩu hiện tại không đúng.");
            }

            if (ModelState.IsValid)
            {
                user.MatKhau = PasswordHelper.HashPassword(model.MatKhauMoi);
                user.NgayCapNhat = DateTime.Now;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                return RedirectToAction(nameof(DoiMatKhau));
            }

            return View("~/Views/TaiKhoan/DoiMatKhau.cshtml", model);
        }

        // ================================================
        // DANH SÁCH QUYỀN CỦA TÔI
        // ================================================

        [Route("QuyenCuaToi")]
        [HttpGet]
        public async Task<IActionResult> QuyenCuaToi()
        {
            var userId = int.Parse(HttpContext.Session.GetString("AdminUserId") ?? "0");
            if (userId <= 0) return RedirectToAction("Login", "Auth");

            ViewBag.IsAdmin = PhanQuyenHelper.IsAdmin(HttpContext.Session);

            var quyens = await _quyenService.GetQuyenCuaNguoiDungAsync(userId);
            var allModules = ModuleQuyen.GetAllModules();

            var model = new List<ModuleQuyenCheckbox>();
            foreach (var module in allModules)
            {
                var prince = quyens.FirstOrDefault(q => q.MaModule == module.MaModule);
                model.Add(new ModuleQuyenCheckbox
                {
                    MaModule = module.MaModule,
                    TenModule = module.TenModule,
                    Icon = module.Icon,
                    MoTa = module.MoTa,
                    DuocChon = prince != null,
                    CoQuyenXem = prince?.CoQuyenXem ?? false,
                    CoQuyenThem = prince?.CoQuyenThem ?? false,
                    CoQuyenSua = prince?.CoQuyenSua ?? false,
                    CoQuyenXoa = prince?.CoQuyenXoa ?? false
                });
            }

            return View("~/Views/TaiKhoan/QuyenCuaToi.cshtml", model);
        }
    }
}