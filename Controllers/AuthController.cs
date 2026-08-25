using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebMTTQ.Models;
using WebMTTQ.Services;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Cryptography;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace WebMTTQ.Controllers
{
    public class AuthController : Controller
    {
        private readonly DataMTTQContext _context;
        private readonly IQuyenTruyCapService _quyenService;
        private readonly IEmailService _emailService;
        private readonly ISystemSettingsService _settingsService;

        // ==== Phase 3: OTP & password-reset hardening constants (no schema change) ====
        private const int MaxOtpAttempts = 5;                          // failed guesses per OTP-window before invalidation
        private const int MaxOtpRequestsPerEmailPerHour = 10;         // "send OTP" limit per email
        private const int MaxOtpRequestsPerIpPerHour = 20;            // "send OTP" limit per IP
        private static readonly int OtpAttemptWindowSeconds = 120;    // 2 min
        private static readonly int OtpRequestWindowSeconds = 3600;   // 1 hour
        private static readonly int ResetWindowSeconds = 300;         // bounded 5 min post-verification reset window

        // ==== Phase 5: Login brute-force protection (no schema change) ====
        private const int MaxLoginFailuresPerIp = 10;                 // failed password attempts per IP / window
        private const int MaxLoginFailuresPerUser = 6;                // failed password attempts per username / window
        private static readonly int LoginWindowSeconds = 900;         // sliding 15-minute window

        // In-memory rate-limit / attempt counters (single-instance only; reset on restart).
        // key -> long[2] = { windowStartTicks, count }
        private static readonly ConcurrentDictionary<string, long[]> _rateCounters = new();
        private static readonly ConcurrentDictionary<string, long[]> _attemptCounters = new();

        public AuthController(DataMTTQContext context, IQuyenTruyCapService quyenService, IEmailService emailService, ISystemSettingsService settingsService)
        {
            _context = context;
            _quyenService = quyenService;
            _emailService = emailService;
            _settingsService = settingsService;
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

            // Kiểm tra xem có tài khoản Admin chính hay không
            // Nếu không có, chuyển hướng tới trang đăng ký tài khoản admin chính
            if (!HasMainAdmin())
            {
                return RedirectToAction(nameof(DangKyAdmin));
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

            // ===== Phase 5: Login brute-force protection =====
            // Chỉ áp dụng cho lần đăng nhập THẤT BẠI; không chặn user bình thường đăng nhập thành công.
            // In-memory fixed-window (single-instance; reset on restart) - tương tự Phase 3.
            var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var normalizedUser = string.IsNullOrWhiteSpace(model.TenDangNhap) ? "" : model.TenDangNhap.Trim();

            // Nếu IP hoặc tài khoản đã vượt ngưỡng thất bại trong cửa sổ hiện tại -> từ chối sớm
            // (không chạy PBKDF2 tốn CPU, chống brute-force).
            if (LoginLimitExceeded($"login_ip:{remoteIp}", MaxLoginFailuresPerIp, LoginWindowSeconds)
                || LoginLimitExceeded($"login_user:{normalizedUser}", MaxLoginFailuresPerUser, LoginWindowSeconds))
            {
                ModelState.AddModelError("", "Quá nhiều lần đăng nhập thất bại. Vui lòng thử lại sau vài phút.");
                return View(model);
            }

            var user = await _context.NguoiDungs
                .Include(u => u.IdvaiTroNavigation)
                .FirstOrDefaultAsync(u => u.TenDangNhap == model.TenDangNhap);

            if (user != null && PasswordHelper.VerifyPassword(model.MatKhau, user.MatKhau))
            {
                // Phase 5: đăng nhập thành công → reset bộ đếm lỗi cho tài khoản/IP này
                // (đảm bảo user hợp lệ không bị khóa vĩnh viễn/kéo dài sau khi fix).
                ResetLoginLimits(remoteIp, normalizedUser);

                // Kiểm tra trạng thái tài khoản
                if (user.TrangThai == "BiXoa" || user.TrangThai == "Khoa")
                {
                    ModelState.AddModelError("", "Tài khoản này đã bị khóa hoặc vô hiệu hóa. Vui lòng liên hệ quản trị viên.");
                    return View(model);
                }

                // Re-hash automatique: si le mot de passe stocké utilise encore les
                // anciens paramètres (10.000 iterations, format legacy), on le re-hashé
                // avec 100.000 iterations dès que l'utilisateur s'est authentifié.
                if (PasswordHelper.NeedsRehash(user.MatKhau))
                {
                    user.MatKhau = PasswordHelper.HashPassword(model.MatKhau);
                    await _context.SaveChangesAsync();
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

            // Phase 5: ghi nhận lần đăng nhập sai (per-IP + per-username).
            RecordLoginFailure($"login_ip:{remoteIp}", MaxLoginFailuresPerIp, LoginWindowSeconds);
            RecordLoginFailure($"login_user:{normalizedUser}", MaxLoginFailuresPerUser, LoginWindowSeconds);

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
        // ĐĂNG KÝ TÀI KHOẢN ADMIN CHÍNH (LẦN ĐẦU CHẠY)
        // ================================================

        [HttpGet]
        public IActionResult DangKyAdmin()
        {
            // Nếu đã có tài khoản Admin chính, chuyển hướng về trang login
            if (HasMainAdmin())
            {
                return RedirectToAction(nameof(Login));
            }

            return View(new DangKyAdminViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKyAdmin(DangKyAdminViewModel model)
        {
            // Nếu đã có tài khoản Admin chính, chuyển hướng về trang login
            if (HasMainAdmin())
            {
                return RedirectToAction(nameof(Login));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Kiểm tra tên đăng nhập đã tồn tại chưa
            if (await _context.NguoiDungs.AnyAsync(u => u.TenDangNhap == model.TenDangNhap))
            {
                ModelState.AddModelError("TenDangNhap", "Tên đăng nhập này đã tồn tại. Vui lòng chọn tên khác.");
                return View(model);
            }

            // Kiểm tra email đã tồn tại chưa (nếu nhập)
            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                var emailExists = await _context.NguoiDungs.AnyAsync(u => u.Email == model.Email);
                if (emailExists)
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng bởi tài khoản khác.");
                    return View(model);
                }
            }

            // Tìm vai trò Admin hệ thống
            var allRoles = await _context.VaiTros.AsNoTracking().ToListAsync();
            var adminRole = allRoles.FirstOrDefault(v => QuyenHelper.IsAdminVaiTro(v.TenVaiTro));

            // Nếu không có vai trò Admin, tạo mới
            if (adminRole == null)
            {
                adminRole = new VaiTro
                {
                    TenVaiTro = "Quản trị viên",
                    QuyenHan = QuyenBitmask.ToanQuyen, // 15 = toàn quyền
                    NgayTao = DateTime.Now,
                    NgayCapNhat = DateTime.Now
                };
                _context.VaiTros.Add(adminRole);
                await _context.SaveChangesAsync();
            }

            // Tạo tài khoản admin chính
            var emailValue = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
            var adminUser = new NguoiDung
            {
                TenDangNhap = model.TenDangNhap.Trim(),
                MatKhau = PasswordHelper.HashPassword(model.MatKhau),
                HoTen = model.HoTen.Trim(),
                Email = emailValue,
                SoDienThoai = string.IsNullOrWhiteSpace(model.SoDienThoai) ? null : model.SoDienThoai.Trim(),
                IdvaiTro = adminRole.IdvaiTro,
                TrangThai = "HoatDong",
                NgayTao = DateTime.Now,
                NgayCapNhat = DateTime.Now
            };

            _context.NguoiDungs.Add(adminUser);
            await _context.SaveChangesAsync();

            // Lưu ID tài khoản Admin chính vào cấu hình hệ thống
            await _settingsService.SetValueAsync("MainAdminId", adminUser.IdnguoiDung.ToString(), "ID người dùng tài khoản Admin chính hệ thống");

            // Đăng nhập tự động
            HttpContext.Session.SetString("AdminLoggedIn", "true");
            HttpContext.Session.SetString("AdminUserId", adminUser.IdnguoiDung.ToString());
            HttpContext.Session.SetString("AdminHoTen", adminUser.HoTen);
            HttpContext.Session.SetString("AdminTenDangNhap", adminUser.TenDangNhap);
            HttpContext.Session.SetString("AdminVaiTro", adminRole.TenVaiTro);

            var quyens = await _quyenService.GetQuyenCuaNguoiDungAsync(adminUser.IdnguoiDung);
            long roleVersion = adminRole.NgayCapNhat?.Ticks ?? adminRole.NgayTao?.Ticks ?? 0;
            PhanQuyenHelper.SaveQuyenToSession(
                HttpContext.Session, quyens, true, adminRole.TenVaiTro, 
                adminRole.IdvaiTro, roleVersion);

            TempData["SuccessMessage"] = "Tài khoản Admin chính đã được tạo thành công!";
            return RedirectToAction("Index", "Admin");
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

            var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // Chống spam: giới hạn "gửi OTP" per-email + per-IP (cửa sổ in-memory).
            if (IsRateLimited($"otp_req_email:{model.Email}", MaxOtpRequestsPerEmailPerHour, OtpRequestWindowSeconds)
                || IsRateLimited($"otp_req_ip:{remoteIp}", MaxOtpRequestsPerIpPerHour, OtpRequestWindowSeconds))
            {
                // Phản hồi chung (không tiết lộ sự tồn tại của tài khoản).
                TempData["InfoMessage"] = "Nếu tài khoản tồn tại, mã xác thực sẽ được gửi đến email của bạn.";
                return RedirectToAction(nameof(XacNhanOtp), new { email = model.Email });
            }

            // Kiểm tra sự tồn tại của tài khoản mà không tiết lộ ra client (chống liệt kê).
            var user = await _context.NguoiDungs
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            // Phản hồi giống nhau cho mọi trạng thái (có/không có tài khoản).
            TempData["InfoMessage"] = "Nếu tài khoản tồn tại, mã xác thực sẽ được gửi đến email của bạn.";

            if (user == null)
            {
                // Tài khoản không tồn tại: không tạo OTP, không gửi email. Redirect giống nhau.
                return RedirectToAction(nameof(XacNhanOtp), new { email = model.Email });
            }

            // Tài khoản tồn tại: không cho gửi lại nếu OTP đang hoạt động còn hơn 90 giây.
            var lastOtp = await _context.MaXacThus
                .Where(o => o.Email == model.Email && !o.DaSuDung)
                .OrderByDescending(o => o.NgayTao)
                .FirstOrDefaultAsync();

            if (lastOtp != null && lastOtp.HanHet > DateTime.Now)
            {
                var remainingSeconds = (int)(lastOtp.HanHet - DateTime.Now).TotalSeconds;
                if (remainingSeconds > 90)
                {
                    return RedirectToAction(nameof(XacNhanOtp), new { email = model.Email });
                }
            }

            // OTP single-active: vô hiệu hóa các OTP chưa dùng trước đó của cùng email.
            await InvalidateActiveOtpsAsync(model.Email);

            // Reset bộ đếm lần nhập sai cho tài khoản này.
            ResetFailedAttempts(model.Email);

            // Tạo OTP 6 số bằng CSPRNG (RandomNumberGenerator) của .NET.
            string otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

            var maXacThuc = new MaXacThuc
            {
                Email = model.Email,
                MaOtp = otp,
                NgayTao = DateTime.Now,
                HanHet = DateTime.Now.AddMinutes(2), // Hiệu lực 2 phút
                DaSuDung = false,
                DiaChiIp = remoteIp
            };

            _context.MaXacThus.Add(maXacThuc);
            await _context.SaveChangesAsync();

            // Gửi email OTP
            var emailSent = await _emailService.SendOtpEmailAsync(model.Email, otp);

            if (emailSent)
            {
                // Chuyển tới trang xác nhận OTP
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
                // Giới hạn số lần nhập sai. Khi vượt quá → vô hiệu hóa OTP để chống brute-force.
                if (IncrementFailedAttempts(model.Email) >= MaxOtpAttempts)
                {
                    await InvalidateActiveOtpsAsync(model.Email);
                    ResetFailedAttempts(model.Email);
                    TempData["ErrorMessage"] = "Đã nhập sai quá nhiều lần. Vui lòng gửi lại mã OTP mới.";
                    return RedirectToAction(nameof(QuenMatKhau));
                }

                ModelState.AddModelError("MaOtp", "Mã OTP không đúng hoặc đã hết hạn. Vui lòng thử lại.");
                return View(model);
            }

            // Đánh dấu OTP đã sử dụng
            otpRecord.DaSuDung = true;
            await _context.SaveChangesAsync();

            // Reset bộ đếm lần nhập sai.
            ResetFailedAttempts(model.Email);

            // Lưu trạng thái reset vào session (không đưa OTP lên URL).
            HttpContext.Session.SetString("ResetPasswordEmail", model.Email);
            HttpContext.Session.SetString("ResetPasswordIssuedAt", DateTime.Now.Ticks.ToString());

            // Chuyển tới trang đặt lại mật khẩu (không kèm OTP trong URL).
            return RedirectToAction(nameof(DatLaiMatKhau));
        }

        // ================================================
        // ĐẶT LẠI MẬT KHẨU (SAU KHI XÁC NHẬN OTP)
        // ================================================

        [HttpGet]
        public IActionResult DatLaiMatKhau()
        {
            var email = HttpContext.Session.GetString("ResetPasswordEmail");
            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Phiên xác thực chưa được thiết lập. Vui lòng thực hiện lại từ đầu.";
                return RedirectToAction(nameof(QuenMatKhau));
            }

            var model = new DatLaiMatKhauViewModel { Email = email };
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

            // Kiểm tra trạng thái reset được lưu trong session (không tin tưởng email do client cung cấp).
            var sessionEmail = HttpContext.Session.GetString("ResetPasswordEmail");
            var issuedAtTicks = HttpContext.Session.GetString("ResetPasswordIssuedAt");

            if (string.IsNullOrEmpty(sessionEmail) || !long.TryParse(issuedAtTicks, out var issuedTicks))
            {
                TempData["ErrorMessage"] = "Phiên xác thực không hợp lệ. Vui lòng thực hiện lại từ đầu.";
                return RedirectToAction(nameof(QuenMatKhau));
            }

            // Ràng buộc email trong session với email đã xác minh (chống chuyển sang tài khoản khác).
            if (!string.Equals(sessionEmail, model.Email, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Phiên xác thực không hợp lệ. Vui lòng thực hiện lại từ đầu.";
                return RedirectToAction(nameof(QuenMatKhau));
            }

            // Chỉ cho phép reset trong cửa sổ ResetWindow sau khi OTP được xác minh
            // (thay thế cho grace 5 phút trước đây → chống replay).
            var issuedAt = new DateTime(issuedTicks);
            if (issuedAt.Ticks > DateTime.Now.AddMinutes(1).Ticks || (DateTime.Now - issuedAt).TotalSeconds > ResetWindowSeconds)
            {
                HttpContext.Session.Remove("ResetPasswordEmail");
                HttpContext.Session.Remove("ResetPasswordIssuedAt");
                TempData["ErrorMessage"] = "Phiên xác thực đã hết hạn. Vui lòng thực hiện lại từ đầu.";
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

            // Vô hiệu hóa mọi OTP đang hoạt động và xóa trạng thái reset → không cho reuse.
            await InvalidateActiveOtpsAsync(model.Email);
            HttpContext.Session.Remove("ResetPasswordEmail");
            HttpContext.Session.Remove("ResetPasswordIssuedAt");

            TempData["SuccessMessage"] = "Đặt lại mật khẩu thành công! Bạn có thể đăng nhập bằng mật khẩu mới.";
            return RedirectToAction(nameof(Login));
        }

        // ================================================
        // HELPERS
        // ================================================

        /// <summary>
        /// Rate limiting per-email / per-IP cho "gửi OTP" (cửa sổ cố định in-memory).
        /// Trả về true nếu yêu cầu đã đạt giới hạn trong cửa sổ hiện tại.
        /// LƯU Ý: chỉ đúng cho single-instance; reset khi khởi động lại.
        /// </summary>
        private bool IsRateLimited(string key, int maxRequests, int windowSeconds)
        {
            var now = DateTime.Now;
            var entry = _rateCounters.GetOrAdd(key, _ => new long[2] { now.Ticks, 0 });

            lock (entry)
            {
                var windowStart = new DateTime(entry[0]);
                if ((now - windowStart).TotalSeconds >= windowSeconds)
                {
                    // Cửa sổ mới.
                    entry[0] = now.Ticks;
                    entry[1] = 0;
                }

                entry[1]++;
                return entry[1] > maxRequests;
            }
        }

        /// <summary>
        /// Tăng bộ đếm số lần nhập sai OTP (per-email, cửa sổ 2 phút).
        /// </summary>
        private int IncrementFailedAttempts(string email)
        {
            var now = DateTime.Now;
            var entry = _attemptCounters.GetOrAdd(email, _ => new long[2] { now.Ticks, 0 });

            lock (entry)
            {
                var windowStart = new DateTime(entry[0]);
                if ((now - windowStart).TotalSeconds >= OtpAttemptWindowSeconds)
                {
                    entry[0] = now.Ticks;
                    entry[1] = 0;
                }

                entry[1]++;
                return (int)entry[1];
            }
        }

        /// <summary>
        /// Xóa bộ đếm lần nhập sai OTP cho email (khi OTP mới được gửi hoặc verify thành công).
        /// </summary>
        private void ResetFailedAttempts(string email)
        {
            _attemptCounters.TryRemove(email, out _);
        }

        // ================= Phase 5: Login brute-force helpers (in-memory, single-instance) =================
        // Chỉ đếm số lần THẤT BẠI. User bình thường đăng nhập thành công không bị ảnh hưởng.

        /// <summary>Peek: true nếu đã đạt/hoặc đang ở trên max trong cửa sổ (không tăng bộ đếm).</summary>
        private bool LoginLimitExceeded(string key, int max, int windowSeconds)
        {
            var now = DateTime.Now;
            var entry = _rateCounters.GetOrAdd(key, _ => new long[2] { now.Ticks, 0 });

            lock (entry)
            {
                var windowStart = new DateTime(entry[0]);
                if ((now - windowStart).TotalSeconds >= windowSeconds) return false; // cửa sổ đã hết
                return entry[1] >= max;
            }
        }

        /// <summary>Tăng bộ đếm lỗi login cho key trong cửa sổ cố định (mui trần tại max).</summary>
        private void RecordLoginFailure(string key, int max, int windowSeconds)
        {
            var now = DateTime.Now;
            var entry = _rateCounters.GetOrAdd(key, _ => new long[2] { now.Ticks, 0 });

            lock (entry)
            {
                var windowStart = new DateTime(entry[0]);
                if ((now - windowStart).TotalSeconds >= windowSeconds)
                {
                    entry[0] = now.Ticks;
                    entry[1] = 0;
                }
                if (entry[1] < max) entry[1]++;
            }
        }

        /// <summary>Xóa bộ đếm lỗi cho tài khoản/IP sau khi đăng nhập thành công.</summary>
        private void ResetLoginLimits(string ip, string username)
        {
            _rateCounters.TryRemove($"login_ip:{ip}", out _);
            _rateCounters.TryRemove($"login_user:{username}", out _);
        }

        /// <summary>
        /// Vô hiệu hóa mọi OTP chưa dùng của email (single-active OTP policy).
        /// </summary>
        private async Task InvalidateActiveOtpsAsync(string email)
        {
            var active = await _context.MaXacThus
                .Where(o => o.Email == email && !o.DaSuDung)
                .ToListAsync();

            foreach (var otp in active)
            {
                otp.DaSuDung = true;
            }

            if (active.Count > 0)
            {
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Kiểm tra xem hệ thống đã có tài khoản Admin chính hay không.
        /// </summary>
        private bool HasMainAdmin()
        {
            var mainAdminId = _settingsService.GetValue("MainAdminId");
            if (string.IsNullOrWhiteSpace(mainAdminId))
            {
                return false;
            }

            // Vérifier que l'utilisateur existe encore
            var adminId = int.TryParse(mainAdminId, out var id) ? id : 0;
            if (adminId <= 0)
            {
                return false;
            }

            return _context.NguoiDungs.Any(u => u.IdnguoiDung == adminId);
        }
    }
}
