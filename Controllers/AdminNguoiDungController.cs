using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebMTTQ.Models;
using WebMTTQ.Services;
using System.Threading.Tasks;

namespace WebMTTQ.Controllers
{
    [Route("AdminNguoiDung")]
    public class AdminNguoiDungController : BaseAdminController
    {
        private readonly DataMTTQContext _context;
        private readonly IQuyenTruyCapService _quyenService;
        private readonly ISystemSettingsService _settingsService;

        public AdminNguoiDungController(DataMTTQContext context, IQuyenTruyCapService quyenService, ISystemSettingsService settingsService)
        {
            _context = context;
            _quyenService = quyenService;
            _settingsService = settingsService;
        }

        // ================================================
        // DANH SÁCH NGƯỜI DÙNG
        // ================================================

        [Route("")]
        [Route("Index")]
        [HttpGet]
        public async Task<IActionResult> Index(string? tuKhoa, int? idVaiTro)
        {
            // Cần quyền QuanLyNguoiDung để xem danh sách người dùng
            if (!PhanQuyenHelper.CoQuyenXem(HttpContext.Session, ModuleQuyen.QuanLyNguoiDung))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            IQueryable<NguoiDung> query = _context.NguoiDungs
                .Include(u => u.IdvaiTroNavigation);

            if (!string.IsNullOrEmpty(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();
                query = query.Where(u => u.TenDangNhap.Contains(tuKhoa) || u.HoTen.Contains(tuKhoa) || (u.Email != null && u.Email.Contains(tuKhoa)));
            }

            if (idVaiTro.HasValue && idVaiTro.Value > 0)
            {
                query = query.Where(u => u.IdvaiTro == idVaiTro.Value);
            }

            var users = await query.OrderByDescending(u => u.NgayTao).ToListAsync();
            var listVaiTro = await _context.VaiTros.ToListAsync();

            // Lấy ID người dùng hiện tại để đánh dấu "chính mình"
            var currentUserId = int.Parse(HttpContext.Session.GetString("AdminUserId") ?? "0");
            var mainAdminId = _settingsService.GetValue("MainAdminId");

            var model = new QuanLyNguoiDungIndexViewModel
            {
                TuKhoa = tuKhoa,
                IdVaiTroFilter = idVaiTro,
                VaiTros = listVaiTro,
                NguoiDungs = users.Select(u => new NguoiDungItem
                {
                    IdnguoiDung = u.IdnguoiDung,
                    TenDangNhap = u.TenDangNhap,
                    HoTen = u.HoTen,
                    Email = u.Email,
                    SoDienThoai = u.SoDienThoai,
                    TenVaiTro = u.IdvaiTroNavigation?.TenVaiTro,
                    TrangThai = u.TrangThai,
                    NgayTao = u.NgayTao,
                    LaAdmin = QuyenHelper.IsAdminVaiTro(u.IdvaiTroNavigation?.TenVaiTro),
                    LaChinhAdmin = u.IdnguoiDung.ToString() == mainAdminId,
                    LaChinhMinh = u.IdnguoiDung == currentUserId
                }).ToList()
            };

            // Đếm số module được quyền cho mỗi người dùng (dựa trên vai trò)
            foreach (var item in model.NguoiDungs)
            {
                var quyens = await _quyenService.GetModulesDuocQuyenAsync(item.IdnguoiDung);
                item.SoModuleDuocQuyen = quyens.Count;
            }

            return View("~/Views/Admin/NguoiDung/Index.cshtml", model);
        }

        // ================================================
        // TẠO NGƯỜI DÙNG MỚI
        // ================================================

        [Route("Create")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!PhanQuyenHelper.CoQuyenThem(HttpContext.Session, ModuleQuyen.QuanLyNguoiDung))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            var model = new TaoNguoiDungViewModel
            {
                VaiTros = await GetVaiTros()
            };
            return View("~/Views/Admin/NguoiDung/Create.cshtml", model);
        }

        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaoNguoiDungViewModel model)
        {
            if (!PhanQuyenHelper.CoQuyenThem(HttpContext.Session, ModuleQuyen.QuanLyNguoiDung))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            model.VaiTros = await GetVaiTros();

            // Kiểm tra tên đăng nhập đã tồn tại chưa
            if (await _context.NguoiDungs.AnyAsync(u => u.TenDangNhap == model.TenDangNhap))
            {
                ModelState.AddModelError("TenDangNhap", "Tên đăng nhập này đã tồn tại. Vui lòng chọn tên khác.");
            }

            if (ModelState.IsValid)
            {
                // Chống role escalation: User không phải Admin không được tạo user với role Admin
                var currentUserIsAdmin = PhanQuyenHelper.IsAdmin(HttpContext.Session);
                if (!currentUserIsAdmin)
                {
                    var selectedRole = model.VaiTros.FirstOrDefault(v => v.IdvaiTro == model.IdVaiTro);
                    if (selectedRole != null && QuyenHelper.IsAdminVaiTro(selectedRole.TenVaiTro))
                    {
                        TempData["ErrorMessage"] = "Bạn không có quyền gán vai trò quản trị viên cho người dùng mới!";
                        return View("~/Views/Admin/NguoiDung/Create.cshtml", model);
                    }
                }

                // Email nullable: nếu không nhập thì để NULL
                // (tránh vi phạm UNIQUE constraint trên Email vì nhiều user có thể không có email)
                var emailValue = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();

                var user = new NguoiDung
                {
                    TenDangNhap = model.TenDangNhap.Trim(),
                    MatKhau = PasswordHelper.HashPassword(model.MatKhau),
                    HoTen = model.HoTen.Trim(),
                    Email = emailValue,
                    SoDienThoai = string.IsNullOrWhiteSpace(model.SoDienThoai) ? null : model.SoDienThoai.Trim(),
                    IdvaiTro = model.IdVaiTro,
                    TrangThai = "HoatDong",
                    NgayTao = DateTime.Now,
                    NgayCapNhat = DateTime.Now
                };

                _context.NguoiDungs.Add(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Tạo tài khoản '{model.TenDangNhap}' thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View("~/Views/Admin/NguoiDung/Create.cshtml", model);
        }

        // ================================================
        // CHỈNH SỬA NGƯỜI DÙNG
        // ================================================

        [Route("Edit/{id}")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!PhanQuyenHelper.CoQuyenSua(HttpContext.Session, ModuleQuyen.QuanLyNguoiDung))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            var user = await _context.NguoiDungs
                .Include(u => u.IdvaiTroNavigation)
                .FirstOrDefaultAsync(u => u.IdnguoiDung == id);

            if (user == null) return NotFound();

            // Chống tương tác với tài khoản Admin: Chỉ Admin mới được sửa tài khoản Admin
            var currentUserIsAdmin = PhanQuyenHelper.IsAdmin(HttpContext.Session);
            var targetIsAdmin = QuyenHelper.IsAdminVaiTro(user.IdvaiTroNavigation?.TenVaiTro);
            if (targetIsAdmin && !currentUserIsAdmin)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền chỉnh sửa tài khoản quản trị viên!";
                return RedirectToAction(nameof(Index));
            }

            // TUYỆT ĐỐI KHÔNG cho phép chỉnh sửa tài khoản Admin chính bởi người khác
            var mainAdminId = _settingsService.GetValue("MainAdminId");
            if (user.IdnguoiDung.ToString() == mainAdminId)
            {
                TempData["ErrorMessage"] = "Không thể chỉnh sửa tài khoản Admin chính qua trang quản lý người dùng!";
                return RedirectToAction(nameof(Index));
            }

            // Admin không được sửa chính tài khoản của mình qua trang quản lý
            // (chỉ được sửa thông tin cá nhân qua trang cá nhân)
            var currentUserId = int.Parse(HttpContext.Session.GetString("AdminUserId") ?? "0");
            if (user.IdnguoiDung == currentUserId)
            {
                TempData["ErrorMessage"] = "Bạn không thể chỉnh sửa tài khoản của chính mình qua trang quản lý người dùng!";
                return RedirectToAction(nameof(Index));
            }

            var model = new SuaNguoiDungViewModel
            {
                IdnguoiDung = user.IdnguoiDung,
                TenDangNhap = user.TenDangNhap,
                HoTen = user.HoTen,
                SoDienThoai = user.SoDienThoai,
                Email = user.Email,
                IdVaiTro = user.IdvaiTro ?? 0,
                TrangThai = user.TrangThai ?? "HoatDong",
                VaiTros = await GetVaiTros()
            };

            return View("~/Views/Admin/NguoiDung/Edit.cshtml", model);
        }

        [Route("Edit/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SuaNguoiDungViewModel model)
        {
            if (!PhanQuyenHelper.CoQuyenSua(HttpContext.Session, ModuleQuyen.QuanLyNguoiDung))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            if (id != model.IdnguoiDung) return NotFound();

            model.VaiTros = await GetVaiTros();

            var user = await _context.NguoiDungs
                .Include(u => u.IdvaiTroNavigation)
                .FirstOrDefaultAsync(u => u.IdnguoiDung == id);

            if (user == null) return NotFound();

            // Chống tương tác với tài khoản Admin: Chỉ Admin mới được sửa tài khoản Admin
            var currentUserIsAdmin = PhanQuyenHelper.IsAdmin(HttpContext.Session);
            var targetIsAdmin = QuyenHelper.IsAdminVaiTro(user.IdvaiTroNavigation?.TenVaiTro);
            if (targetIsAdmin && !currentUserIsAdmin)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền chỉnh sửa tài khoản quản trị viên!";
                return RedirectToAction(nameof(Index));
            }

            // TUYỆT ĐỐI KHÔNG cho phép chỉnh sửa tài khoản Admin chính bởi người khác
            var mainAdminId = _settingsService.GetValue("MainAdminId");
            if (user.IdnguoiDung.ToString() == mainAdminId)
            {
                TempData["ErrorMessage"] = "Không thể chỉnh sửa tài khoản Admin chính qua trang quản lý người dùng!";
                return RedirectToAction(nameof(Index));
            }

            // Admin không được sửa chính tài khoản của mình qua trang quản lý
            var currentUserId = int.Parse(HttpContext.Session.GetString("AdminUserId") ?? "0");
            if (user.IdnguoiDung == currentUserId)
            {
                TempData["ErrorMessage"] = "Bạn không thể chỉnh sửa tài khoản của chính mình qua trang quản lý người dùng!";
                return RedirectToAction(nameof(Index));
            }

            // Chống role escalation: Nếu user đang được sửa có vai trò Admin và người sửa không phải Admin
            // thì không cho phép thay đổi vai trò.
            if (!currentUserIsAdmin)
            {
                // Nếu người sửa cố gán quyền Admin cho user khác → chặn
                var targetRole = model.VaiTros.FirstOrDefault(v => v.IdvaiTro == model.IdVaiTro);
                if (targetRole != null && QuyenHelper.IsAdminVaiTro(targetRole.TenVaiTro))
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền gán vai trò quản trị viên!";
                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                // Admin không được hạ quyền chính mình (đã chặn ở trên vì không cho sửa chính mình)
                // Admin không được hạ quyền tài khoản Admin khác xuống role thường
                var targetRole = model.VaiTros.FirstOrDefault(v => v.IdvaiTro == model.IdVaiTro);
                if (targetIsAdmin && targetRole != null && !QuyenHelper.IsAdminVaiTro(targetRole.TenVaiTro))
                {
                    TempData["ErrorMessage"] = "Không thể hạ quyền tài khoản quản trị viên xuống vai trò thường!";
                    return RedirectToAction(nameof(Index));
                }
            }

            // Không cho phép chỉnh sửa tên đăng nhập (đã cố định)
            model.TenDangNhap = user.TenDangNhap;

            // Kiểm tra password confirm nếu có nhập mật khẩu mới
            if (!string.IsNullOrEmpty(model.MatKhauMoi) && model.MatKhauMoi != model.XacNhanMatKhauMoi)
            {
                ModelState.AddModelError("XacNhanMatKhauMoi", "Mật khẩu xác nhận không khớp.");
            }

            if (ModelState.IsValid)
            {
                user.HoTen = model.HoTen;
                user.SoDienThoai = model.SoDienThoai;
                user.Email = model.Email;
                user.IdvaiTro = model.IdVaiTro;
                user.TrangThai = model.TrangThai;
                user.NgayCapNhat = DateTime.Now;

                // Đổi mật khẩu nếu được yêu cầu
                if (!string.IsNullOrEmpty(model.MatKhauMoi))
                {
                    user.MatKhau = PasswordHelper.HashPassword(model.MatKhauMoi);
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Đã cập nhật tài khoản '{user.TenDangNhap}' thành công!";
                return RedirectToAction(nameof(Index));
            }

            model.TenDangNhap = user.TenDangNhap;
            return View("~/Views/Admin/NguoiDung/Edit.cshtml", model);
        }

        // ================================================
        // XÓA NGƯỜI DÙNG (SOFT DELETE)
        // ================================================

        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!PhanQuyenHelper.CoQuyenXoa(HttpContext.Session, ModuleQuyen.QuanLyNguoiDung))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            var user = await _context.NguoiDungs
                .Include(u => u.IdvaiTroNavigation)
                .FirstOrDefaultAsync(u => u.IdnguoiDung == id);

            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng này!";
                return RedirectToAction(nameof(Index));
            }

            // Không cho phép xóa chính mình
            var currentUserId = int.Parse(HttpContext.Session.GetString("AdminUserId") ?? "0");
            if (user.IdnguoiDung == currentUserId)
            {
                TempData["ErrorMessage"] = "Bạn không thể xóa tài khoản của chính mình!";
                return RedirectToAction(nameof(Index));
            }

            // TUYỆT ĐỐI KHÔNG cho phép xóa tài khoản Admin chính
            var mainAdminId = _settingsService.GetValue("MainAdminId");
            if (user.IdnguoiDung.ToString() == mainAdminId)
            {
                TempData["ErrorMessage"] = "Không thể xóa tài khoản Admin chính! Tài khoản này là bất khả xâm phạm.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra quyền xóa tài khoản Admin
            var targetIsAdmin = QuyenHelper.IsAdminVaiTro(user.IdvaiTroNavigation?.TenVaiTro);
            if (targetIsAdmin)
            {
                var currentUser = await _context.NguoiDungs
                    .Include(u => u.IdvaiTroNavigation)
                    .FirstOrDefaultAsync(u => u.IdnguoiDung == currentUserId);

                var currentUserIsAdmin = currentUser != null && QuyenHelper.IsAdminVaiTro(currentUser.IdvaiTroNavigation?.TenVaiTro);
                var currentUserIsChinhAdmin = currentUser != null && currentUser.IdnguoiDung.ToString() == _settingsService.GetValue("MainAdminId");

                // Chỉ Admin chính mới được xóa tài khoản Admin phụ
                if (!currentUserIsChinhAdmin)
                {
                    TempData["ErrorMessage"] = "Chỉ Admin chính mới có quyền xóa tài khoản quản trị viên khác!";
                    return RedirectToAction(nameof(Index));
                }
            }

            // Hard delete
            _context.NguoiDungs.Remove(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã xóa tài khoản '{user.TenDangNhap}' thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ================================================
        // XEM CHI TIẾT NGƯỜI DÙNG
        // ================================================

        [Route("Details/{id}")]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (!PhanQuyenHelper.CoQuyenXem(HttpContext.Session, ModuleQuyen.QuanLyNguoiDung))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            var user = await _context.NguoiDungs
                .Include(u => u.IdvaiTroNavigation)
                .FirstOrDefaultAsync(u => u.IdnguoiDung == id);

            if (user == null) return NotFound();

            // Chống tương tác với tài khoản Admin: Chỉ Admin mới được xem chi tiết tài khoản Admin
            var currentUserIsAdmin = PhanQuyenHelper.IsAdmin(HttpContext.Session);
            var targetIsAdmin = QuyenHelper.IsAdminVaiTro(user.IdvaiTroNavigation?.TenVaiTro);
            if (targetIsAdmin && !currentUserIsAdmin)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xem chi tiết tài khoản quản trị viên!";
                return RedirectToAction(nameof(Index));
            }

            // Lấy quyền từ vai trò của user
            var quyens = await _quyenService.GetQuyenCuaNguoiDungAsync(id);
            ViewBag.Quyens = quyens;
            ViewBag.IsAdmin = QuyenHelper.IsAdminVaiTro(user.IdvaiTroNavigation?.TenVaiTro);
            ViewBag.LaChinhMinh = user.IdnguoiDung == int.Parse(HttpContext.Session.GetString("AdminUserId") ?? "0");
            ViewBag.LaChinhAdmin = user.IdnguoiDung.ToString() == _settingsService.GetValue("MainAdminId");

            return View("~/Views/Admin/NguoiDung/Details.cshtml", user);
        }

        // ================================================
        // QUẢN LÝ VAI TRÒ
        // ================================================

        [Route("VaiTro")]
        [HttpGet]
        public async Task<IActionResult> VaiTro()
        {
            if (!PhanQuyenHelper.CoQuyenXem(HttpContext.Session, ModuleQuyen.QuanLyNguoiDung))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            var vaiTros = await _context.VaiTros
                .OrderBy(v => v.TenVaiTro)
                .ToListAsync();

            // Đếm số module được quyền cho từng vai trò từ bảng VaiTroQuyen
            var moduleCounts = new Dictionary<int, int>();
            foreach (var vt in vaiTros)
            {
                var quyens = await _quyenService.GetQuyenCuaVaiTroAsync(vt.IdvaiTro);
                moduleCounts[vt.IdvaiTro] = quyens.Count;
            }
            ViewBag.ModuleCounts = moduleCounts;

            // Đếm số người dùng cho từng vai trò
            var userCounts = new Dictionary<int, int>();
            foreach (var vt in vaiTros)
            {
                var count = await _context.NguoiDungs.CountAsync(u => u.IdvaiTro == vt.IdvaiTro);
                userCounts[vt.IdvaiTro] = count;
            }
            ViewBag.UserCounts = userCounts;

            return View("~/Views/Admin/NguoiDung/VaiTro.cshtml", vaiTros);
        }

        [Route("VaiTro/Create")]
        [HttpGet]
        public IActionResult VaiTroCreate()
        {
            if (!PhanQuyenHelper.CoQuyenThem(HttpContext.Session, ModuleQuyen.QuanLyNguoiDung))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            var model = new VaiTroViewModel
            {
                Modules = BuildModuleCheckboxes(new List<ModuleQuyenInfo>())
            };
            return View("~/Views/Admin/NguoiDung/VaiTroCreate.cshtml", model);
        }

        [Route("VaiTro/Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VaiTroCreate(VaiTroViewModel model)
        {
            if (!PhanQuyenHelper.CoQuyenThem(HttpContext.Session, ModuleQuyen.QuanLyNguoiDung))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            model.Modules = ParseModulesFromForm(Request);

            if (ModelState.IsValid)
            {
                // Kiểm tra tên vai trò đã tồn tại
                if (await _context.VaiTros.AnyAsync(v => v.TenVaiTro == model.TenVaiTro))
                {
                    ModelState.AddModelError("TenVaiTro", "Tên vai trò này đã tồn tại.");
                    return View("~/Views/Admin/NguoiDung/VaiTroCreate.cshtml", model);
                }

                var vaiTro = new VaiTro
                {
                    TenVaiTro = model.TenVaiTro.Trim(),
                    // QuyenHan sẽ được set chính xác trong SaveQuyenChoVaiTroAsync
                    QuyenHan = QuyenHelper.IsAdminVaiTro(model.TenVaiTro) ? QuyenBitmask.ToanQuyen : (byte)0,
                    NgayTao = DateTime.Now,
                    NgayCapNhat = DateTime.Now
                };

                _context.VaiTros.Add(vaiTro);
                await _context.SaveChangesAsync();

                // Lưu quyền chi tiết theo module - chỉ lưu các module được chọn
                // VaiTroQuyen: xóa hết quyền cũ và chèn đúng các module đã tick
                await _quyenService.SaveQuyenChoVaiTroAsync(vaiTro.IdvaiTro, ConvertCheckboxesToQuyenInfos(model.Modules));

                TempData["SuccessMessage"] = $"Tạo vai trò '{model.TenVaiTro}' thành công!";
                return RedirectToAction(nameof(VaiTro));
            }

            return View("~/Views/Admin/NguoiDung/VaiTroCreate.cshtml", model);
        }

        [Route("VaiTro/Edit/{id}")]
        [HttpGet]
        public async Task<IActionResult> VaiTroEdit(int id)
        {
            if (!PhanQuyenHelper.CoQuyenSua(HttpContext.Session, ModuleQuyen.QuanLyNguoiDung))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            var vaiTro = await _context.VaiTros.FindAsync(id);
            if (vaiTro == null) return NotFound();

            var quyens = await _quyenService.GetQuyenCuaVaiTroAsync(id);

            var model = new VaiTroViewModel
            {
                IdvaiTro = vaiTro.IdvaiTro,
                TenVaiTro = vaiTro.TenVaiTro,
                Modules = BuildModuleCheckboxes(quyens)
            };

            return View("~/Views/Admin/NguoiDung/VaiTroEdit.cshtml", model);
        }

        [Route("VaiTro/Edit/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VaiTroEdit(int id, VaiTroViewModel model)
        {
            if (!PhanQuyenHelper.CoQuyenSua(HttpContext.Session, ModuleQuyen.QuanLyNguoiDung))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            if (id != model.IdvaiTro) return NotFound();

            model.Modules = ParseModulesFromForm(Request);

            var vaiTro = await _context.VaiTros.FindAsync(id);
            if (vaiTro == null) return NotFound();

            // Chống hack: Chỉ Admin mới được sửa role Admin
            var currentUserIsAdmin = PhanQuyenHelper.IsAdmin(HttpContext.Session);
            var targetIsAdminRole = QuyenHelper.IsAdminVaiTro(vaiTro.TenVaiTro) || QuyenHelper.IsAdminVaiTro(model.TenVaiTro);
            if (targetIsAdminRole && !currentUserIsAdmin)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền chỉnh sửa vai trò quản trị viên!";
                return RedirectToAction(nameof(VaiTro));
            }

            // Chống bypass: Không cho phép đổi tên role Admin thành tên khác
            // (để tránh mất quyền Admin và bypass kiểm tra IsAdminVaiTro)
            var roleIsCurrentlyAdmin = QuyenHelper.IsAdminVaiTro(vaiTro.TenVaiTro);
            var roleWillBeAdmin = QuyenHelper.IsAdminVaiTro(model.TenVaiTro);
            if (roleIsCurrentlyAdmin && !roleWillBeAdmin)
            {
                TempData["ErrorMessage"] = "Không thể đổi tên vai trò Quản trị viên hệ thống! Tên vai trò Admin là bất khả xâm phạm.";
                return RedirectToAction(nameof(VaiTro));
            }

            if (ModelState.IsValid)
            {
                vaiTro.TenVaiTro = model.TenVaiTro.Trim();
                vaiTro.NgayCapNhat = DateTime.Now;

                // QuyenHan được set chính xác trong SaveQuyenChoVaiTroAsync
                if (!QuyenHelper.IsAdminVaiTro(model.TenVaiTro))
                {
                    vaiTro.QuyenHan = 0;
                }

                await _context.SaveChangesAsync();

                // Lưu quyền chi tiết theo module - chỉ lưu các module được chọn
                // VaiTroQuyen: xóa hết quyền cũ và chèn đúng các module đã tick
                await _quyenService.SaveQuyenChoVaiTroAsync(id, ConvertCheckboxesToQuyenInfos(model.Modules));

                TempData["SuccessMessage"] = $"Đã cập nhật vai trò '{model.TenVaiTro}' thành công!";
                return RedirectToAction(nameof(VaiTro));
            }

            return View("~/Views/Admin/NguoiDung/VaiTroEdit.cshtml", model);
        }

        [HttpPost("VaiTro/Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VaiTroDelete(int id)
        {
            if (!PhanQuyenHelper.CoQuyenXoa(HttpContext.Session, ModuleQuyen.QuanLyNguoiDung))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            // Chống xóa role Admin hệ thống
            var vaiTroToCheck = await _context.VaiTros.FindAsync(id);
            if (vaiTroToCheck != null && QuyenHelper.IsAdminVaiTro(vaiTroToCheck.TenVaiTro))
            {
                TempData["ErrorMessage"] = "Không thể xóa vai trò Quản trị viên hệ thống!";
                return RedirectToAction(nameof(VaiTro));
            }

            var vaiTro = await _context.VaiTros.FindAsync(id);
            if (vaiTro == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy vai trò này!";
                return RedirectToAction(nameof(VaiTro));
            }

            // Kiểm tra có user nào đang dùng vai trò này không
            var users = await _context.NguoiDungs
                .Where(u => u.IdvaiTro == id)
                .Select(u => u.TenDangNhap)
                .ToListAsync();

            if (users.Count > 0)
            {
                var usernames = string.Join(", ", users.Select(u => $"\"{u}\""));
                TempData["ErrorMessage"] = $"Không thể xóa vai trò \"{vaiTro.TenVaiTro}\" vì có {users.Count} người dùng đang sử dụng: {usernames}. Vui lòng chuyển vai trò của các người dùng này sang vai trò khác trước khi xóa.";
                return RedirectToAction(nameof(VaiTro));
            }

            // Hard delete
            _context.VaiTros.Remove(vaiTro);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã xóa vai trò '{vaiTro.TenVaiTro}' thành công!";
            return RedirectToAction(nameof(VaiTro));
        }

        // ================================================
        // HELPERS
        // ================================================

        private async Task<List<VaiTro>> GetVaiTros()
        {
            return await _context.VaiTros
                .OrderBy(v => v.TenVaiTro)
                .ToListAsync();
        }

        private List<ModuleQuyenCheckbox> BuildModuleCheckboxes(List<ModuleQuyenInfo> existingQuyens)
        {
            var modules = ModuleQuyen.GetAllModules();
            var result = new List<ModuleQuyenCheckbox>();

            foreach (var module in modules)
            {
                var existing = existingQuyens.FirstOrDefault(q => q.MaModule == module.MaModule);
                result.Add(new ModuleQuyenCheckbox
                {
                    MaModule = module.MaModule,
                    TenModule = module.TenModule,
                    Icon = module.Icon,
                    MoTa = module.MoTa,
                    DuocChon = existing != null,
                    CoQuyenXem = existing?.CoQuyenXem ?? false,
                    CoQuyenThem = existing?.CoQuyenThem ?? false,
                    CoQuyenSua = existing?.CoQuyenSua ?? false,
                    CoQuyenXoa = existing?.CoQuyenXoa ?? false
                });
            }

            return result;
        }

        private List<ModuleQuyenCheckbox> ParseModulesFromForm(HttpRequest request)
        {
            var modules = ModuleQuyen.GetAllModules();
            var result = new List<ModuleQuyenCheckbox>();

            foreach (var module in modules)
            {
                var checkKey = $"Modules[{module.MaModule}].DuocChon";
                var xemKey = $"Modules[{module.MaModule}].CoQuyenXem";
                var themKey = $"Modules[{module.MaModule}].CoQuyenThem";
                var suaKey = $"Modules[{module.MaModule}].CoQuyenSua";
                var xoaKey = $"Modules[{module.MaModule}].CoQuyenXoa";

                // Checkbox không được chọn sẽ không được gửi trong form data.
                // Chỉ coi là được chọn khi giá trị gửi lên là "true" hoặc "on".
                var duocChon = request.Form[checkKey].FirstOrDefault() == "true" ||
                               request.Form[checkKey].FirstOrDefault() == "on";

                result.Add(new ModuleQuyenCheckbox
                {
                    MaModule = module.MaModule,
                    TenModule = module.TenModule,
                    Icon = module.Icon,
                    MoTa = module.MoTa,
                    DuocChon = duocChon,
                    CoQuyenXem = duocChon && (request.Form[xemKey].FirstOrDefault() == "true" || request.Form[xemKey].FirstOrDefault() == "on"),
                    CoQuyenThem = duocChon && (request.Form[themKey].FirstOrDefault() == "true" || request.Form[themKey].FirstOrDefault() == "on"),
                    CoQuyenSua = duocChon && (request.Form[suaKey].FirstOrDefault() == "true" || request.Form[suaKey].FirstOrDefault() == "on"),
                    CoQuyenXoa = duocChon && (request.Form[xoaKey].FirstOrDefault() == "true" || request.Form[xoaKey].FirstOrDefault() == "on")
                });
            }

            return result;
        }

        private byte BuildQuyenHanFromModules(List<ModuleQuyenCheckbox> modules)
        {
            byte result = 0;
            foreach (var m in modules)
            {
                if (!m.DuocChon) continue;
                if (m.CoQuyenXem) result |= QuyenBitmask.Xem;
                if (m.CoQuyenThem) result |= QuyenBitmask.Them;
                if (m.CoQuyenSua) result |= QuyenBitmask.Sua;
                if (m.CoQuyenXoa) result |= QuyenBitmask.Xoa;
            }
            return result;
        }

        private List<ModuleQuyenInfo> ConvertCheckboxesToQuyenInfos(List<ModuleQuyenCheckbox> modules)
        {
            var result = new List<ModuleQuyenInfo>();
            foreach (var m in modules)
            {
                if (!m.DuocChon) continue;
                result.Add(new ModuleQuyenInfo
                {
                    MaModule = m.MaModule,
                    TenModule = m.TenModule,
                    CoQuyenXem = m.CoQuyenXem,
                    CoQuyenThem = m.CoQuyenThem,
                    CoQuyenSua = m.CoQuyenSua,
                    CoQuyenXoa = m.CoQuyenXoa
                });
            }
            return result;
        }
    }
}