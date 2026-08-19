using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebMTTQ.Models;
using WebMTTQ.Services;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace WebMTTQ.Controllers
{
    public class AuthController : Controller
    {
        private readonly DataMTTQContext _context;
        private readonly IQuyenTruyCapService _quyenService;
        private readonly IEmailService _emailService;

        public AuthController(DataMTTQContext context, IQuyenTruyCapService quyenService, IEmailService emailService)
        {
            _context = context;
            _quyenService = quyenService;
            _emailService = emailService;
        }

        // ================================================
        // ĐĂNG NHẬP
        // ================================================

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
                .Include(u => u.IdvaiTroNavigation)
                .FirstOrDefaultAsync(u => u.TenDangNhap == model.TenDangNhap);

            if (user != null && PasswordHelper.VerifyPassword(model.MatKhau, user.MatKhau))
            {
                // Kiểm tra trạng thái tài khoản
                if (user.TrangThai == "BiXoa" || user.TrangThai == "Khoa")
                {
                    ModelState.AddModelError("", "Tài khoản này đã bị khóa hoặc vô hiệu hóa. Vui lòng liên hệ quản trị viên.");
                    return View(model);
                }

                // Kiểm tra xem có phải Admin không
                var tenVaiTro = user.IdvaiTroNavigation?.TenVaiTro;
                var isAdmin = QuyenHelper.IsAdminVaiTro(tenVaiTro);

                // Lưu thông tin vào session
                HttpContext.Session.SetString("AdminLoggedIn", "true");
                HttpContext.Session.SetString("AdminUserId", user.IdnguoiDung.ToString());
                HttpContext.Session.SetString("AdminHoTen", user.HoTen);
                HttpContext.Session.SetString("AdminTenDangNhap", user.TenDangNhap);
                HttpContext.Session.SetString("AdminVaiTro", tenVaiTro ?? "");

                // Lấy quyền truy cập và lưu vào session
                var quyens = await _quyenService.GetQuyenCuaNguoiDungAsync(user.IdnguoiDung);
                
                // Tính version giống logic trong PhanQuyenHelper.RefreshSessionQuyenIfNeededAsync
                // để đảm bảo nhất quán giữa login và refresh.
                long roleVersion = 0;
                if (user.IdvaiTroNavigation?.NgayCapNhat.HasValue == true)
                {
                    roleVersion = user.IdvaiTroNavigation.NgayCapNhat.Value.Ticks;
                }
                else if (user.IdvaiTroNavigation?.NgayTao.HasValue == true)
                {
                    roleVersion = user.IdvaiTroNavigation.NgayTao.Value.Ticks;
                }
                else
                {
                    // Legacy role: cả NgayCapNhat và NgayTao đều null
                    // Dùng count VaiTroQuyen + roleId factor làm version
                    var quyenCount = await _context.VaiTroQuyens.CountAsync(q => q.IdVaiTro == (user.IdvaiTro ?? 0));
                    long roleIdFactor = (long)(user.IdvaiTro ?? 0) * 1000000;
                    roleVersion = roleIdFactor + quyenCount + 1;
                }

                PhanQuyenHelper.SaveQuyenToSession(
                    HttpContext.Session, quyens, isAdmin, tenVaiTro, 
                    user.IdvaiTro ?? 0, roleVersion);

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

        // ================================================
        // QUÊN MẬT KHẨU - BƯỚC 1: NHẬP EMAIL
        // ================================================

        [HttpGet]
        public IActionResult QuenMatKhau()
        {
            return View(new QuenMatKhauViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuenMatKhau(QuenMatKhauViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.Email = model.Email.Trim().ToLower();

            // Kiểm tra email có tồn tại trong hệ thống không
            var user = await _context.NguoiDungs
                .Include(u => u.IdvaiTroNavigation)
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                // Email không tồn tại - hiển thị thông báo trên trang quên mật khẩu
                model.IsEmailExists = false;
                return View(model);
            }

            // Email tồn tại - hiển thị thông báo thành công
            model.IsEmailExists = true;

            // Kiểm tra thời gian gửi lại OTP (tối thiểu 2 phút giữa các lần gửi)
            var lastOtp = await _context.MaXacThus
                .Where(o => o.Email == model.Email && !o.DaSuDung)
                .OrderByDescending(o => o.NgayTao)
                .FirstOrDefaultAsync();

            if (lastOtp != null && lastOtp.HanHet > DateTime.Now)
            {
                var remainingSeconds = (int)(lastOtp.HanHet - DateTime.Now).TotalSeconds;
                if (remainingSeconds > 90) // Chỉ còn < 30s thì cho gửi lại
                {
                    TempData["ErrorMessage"] = $"Mã OTP đã được gửi gần đây. Vui lòng đợi {Math.Ceiling(remainingSeconds / 60.0)} phút để gửi lại.";
                    return RedirectToAction(nameof(XacNhanOtp), new { email = model.Email });
                }
            }

            // Tạo mã OTP 6 số
            Random random = new Random();
            string otp = random.Next(100000, 999999).ToString();

            var maXacThuc = new MaXacThuc
            {
                Email = model.Email,
                MaOtp = otp,
                NgayTao = DateTime.Now,
                HanHet = DateTime.Now.AddMinutes(2), // Tồn tại 2 phút
                DaSuDung = false,
                DiaChiIp = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            _context.MaXacThus.Add(maXacThuc);
            await _context.SaveChangesAsync();

            // Gửi email OTP
            var emailSent = await _emailService.SendOtpEmailAsync(model.Email, otp);

            if (emailSent)
            {
                // Chuyển hướng tới trang xác nhận OTP nơi ô nhập mã OTP hiển thị
                return RedirectToAction(nameof(XacNhanOtp), new { email = model.Email });
            }

            // Nếu gửi email thất bại, xóa OTP vừa tạo để tránh rác
            _context.MaXacThus.Remove(maXacThuc);
            await _context.SaveChangesAsync();

            TempData["ErrorMessage"] = "Đã có lỗi khi gửi email. Vui lòng kiểm tra cấu hình SMTP trong trang quản trị hoặc thử lại sau.";
            return RedirectToAction(nameof(QuenMatKhau));
        }

        // ================================================
        // XÁC NHẬN OTP
        // ================================================

        [HttpGet]
        public IActionResult XacNhanOtp(string email)
        {
            var model = new XacNhanOtpViewModel { Email = email ?? "" };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanOtp(XacNhanOtpViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.Email = model.Email.Trim().ToLower();
            model.MaOtp = model.MaOtp.Trim();

            // Kiểm tra OTP trong database
            var otpRecord = await _context.MaXacThus
                .Where(o => o.Email == model.Email
                         && o.MaOtp == model.MaOtp
                         && !o.DaSuDung
                         && o.HanHet > DateTime.Now)
                .OrderByDescending(o => o.NgayTao)
                .FirstOrDefaultAsync();

            if (otpRecord == null)
            {
                ModelState.AddModelError("MaOtp", "Mã OTP không đúng hoặc đã hết hạn. Vui lòng thử lại.");
                return View(model);
            }

            // Đánh dấu OTP đã sử dụng
            otpRecord.DaSuDung = true;
            await _context.SaveChangesAsync();

            // Chuyển tới trang đặt lại mật khẩu
            return RedirectToAction(nameof(DatLaiMatKhau), new { email = model.Email, otp = model.MaOtp });
        }

        // ================================================
        // ĐẶT LẠI MẬT KHẨU (SAU KHI XÁC NHẬN OTP)
        // ================================================

        [HttpGet]
        public IActionResult DatLaiMatKhau(string email, string otp)
        {
            var model = new DatLaiMatKhauViewModel
            {
                Email = email ?? "",
                MaOtp = otp ?? ""
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DatLaiMatKhau(DatLaiMatKhauViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.Email = model.Email.Trim().ToLower();
            model.MaOtp = model.MaOtp.Trim();

            // Kiểm tra OTP đã được xác nhận (đã đánh dấu DaSuDung = true)
            var otpRecord = await _context.MaXacThus
                .Where(o => o.Email == model.Email
                         && o.MaOtp == model.MaOtp
                         && o.DaSuDung
                         && o.HanHet > DateTime.Now.AddMinutes(-5)) // Cho phép trong vòng 5 phút kể từ khi OTP hết hạn
                .OrderByDescending(o => o.NgayTao)
                .FirstOrDefaultAsync();

            if (otpRecord == null)
            {
                TempData["ErrorMessage"] = "Phiên xác thực không hợp lệ. Vui lòng thực hiện lại từ đầu.";
                return RedirectToAction(nameof(QuenMatKhau));
            }

            // Tìm và cập nhật mật khẩu
            var user = await _context.NguoiDungs
                .Include(u => u.IdvaiTroNavigation)
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài khoản với email này.";
                return RedirectToAction(nameof(QuenMatKhau));
            }

            user.MatKhau = PasswordHelper.HashPassword(model.MatKhauMoi);
            user.NgayCapNhat = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đặt lại mật khẩu thành công! Bạn có thể đăng nhập bằng mật khẩu mới.";
            return RedirectToAction(nameof(Login));
        }

        // ================================================
        // HELPERS
        // ================================================
    }
}