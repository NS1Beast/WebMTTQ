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

        public AdminNguoiDungController(DataMTTQContext context, IQuyenTruyCapService quyenService)
        {
            _context = context;
            _quyenService = quyenService;
        }

        // ================================================
        // DANH SÁCH NGƯỜI DÙNG
        // ================================================

        [Route("")]
        [Route("Index")]
        [HttpGet]
        public async Task<IActionResult> Index(string? tuKhoa, int? idVaiTro)
        {
            // Chỉ admin mới có quyền quản lý người dùng
            if (!PhanQuyenHelper.IsAdmin(HttpContext.Session))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập vào chức năng này!";
                return RedirectToAction("Index", "Home");
            }

            var query = _context.NguoiDungs
                .Include(u => u.IdvaiTroNavigation)
                .Where(u => u.DaXoa == null || u.DaXoa == false);

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
            var listVaiTro = await _context.VaiTros.Where(v => v.DaXoa == null || v.DaXoa == false).ToListAsync();

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
                    LaAdmin = QuyenHelper.IsAdminVaiTro(u.IdvaiTroNavigation?.TenVaiTro)
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
            if (!PhanQuyenHelper.IsAdmin(HttpContext.Session))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập vào chức năng này!";
                return RedirectToAction("Index", "Home");
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
            if (!PhanQuyenHelper.IsAdmin(HttpContext.Session))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập vào chức năng này!";
                return RedirectToAction("Index", "Home");
            }

            model.VaiTros = await GetVaiTros();

            // Kiểm tra tên đăng nhập đã tồn tại chưa
            if (await _context.NguoiDungs.AnyAsync(u => u.TenDangNhap == model.TenDangNhap))
            {
                ModelState.AddModelError("TenDangNhap", "Tên đăng nhập này đã tồn tại. Vui lòng chọn tên khác.");
            }

            if (ModelState.IsValid)
            {
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
                    DaXoa = false,
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
            if (!PhanQuyenHelper.IsAdmin(HttpContext.Session))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập vào chức năng này!";
                return RedirectToAction("Index", "Home");
            }

            var user = await _context.NguoiDungs
                .Include(u => u.IdvaiTroNavigation)
                .FirstOrDefaultAsync(u => u.IdnguoiDung == id && (u.DaXoa == null || u.DaXoa == false));

            if (user == null) return NotFound();

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
            if (!PhanQuyenHelper.IsAdmin(HttpContext.Session))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập vào chức năng này!";
                return RedirectToAction("Index", "Home");
            }

            if (id != model.IdnguoiDung) return NotFound();

            model.VaiTros = await GetVaiTros();

            var user = await _context.NguoiDungs
                .Include(u => u.IdvaiTroNavigation)
                .FirstOrDefaultAsync(u => u.IdnguoiDung == id && (u.DaXoa == null || u.DaXoa == false));

            if (user == null) return NotFound();

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
            if (!PhanQuyenHelper.IsAdmin(HttpContext.Session))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập vào chức năng này!";
                return RedirectToAction("Index", "Home");
            }

            var user = await _context.NguoiDungs.FindAsync(id);
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

            // Soft delete
            user.DaXoa = true;
            user.TrangThai = "BiXoa";
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
            if (!PhanQuyenHelper.IsAdmin(HttpContext.Session))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập vào chức năng này!";
                return RedirectToAction("Index", "Home");
            }

            var user = await _context.NguoiDungs
                .Include(u => u.IdvaiTroNavigation)
                .FirstOrDefaultAsync(u => u.IdnguoiDung == id && (u.DaXoa == null || u.DaXoa == false));

            if (user == null) return NotFound();

            // Lấy quyền từ vai trò của user
            var quyens = await _quyenService.GetQuyenCuaNguoiDungAsync(id);
            ViewBag.Quyens = quyens;
            ViewBag.IsAdmin = QuyenHelper.IsAdminVaiTro(user.IdvaiTroNavigation?.TenVaiTro);

            return View("~/Views/Admin/NguoiDung/Details.cshtml", user);
        }

        // ================================================
        // QUẢN LÝ VAI TRÒ
        // ================================================

        [Route("VaiTro")]
        [HttpGet]
        public async Task<IActionResult> VaiTro()
        {
            if (!PhanQuyenHelper.IsAdmin(HttpContext.Session))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập vào chức năng này!";
                return RedirectToAction("Index", "Home");
            }

            var vaiTros = await _context.VaiTros
                .Where(v => v.DaXoa == null || v.DaXoa == false)
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

            return View("~/Views/Admin/NguoiDung/VaiTro.cshtml", vaiTros);
        }

        [Route("VaiTro/Create")]
        [HttpGet]
        public IActionResult VaiTroCreate()
        {
            if (!PhanQuyenHelper.IsAdmin(HttpContext.Session))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập vào chức năng này!";
                return RedirectToAction("Index", "Home");
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
            if (!PhanQuyenHelper.IsAdmin(HttpContext.Session))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập vào chức năng này!";
                return RedirectToAction("Index", "Home");
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
                    DaXoa = false,
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
            if (!PhanQuyenHelper.IsAdmin(HttpContext.Session))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập vào chức năng này!";
                return RedirectToAction("Index", "Home");
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
            if (!PhanQuyenHelper.IsAdmin(HttpContext.Session))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập vào chức năng này!";
                return RedirectToAction("Index", "Home");
            }

            if (id != model.IdvaiTro) return NotFound();

            model.Modules = ParseModulesFromForm(Request);

            var vaiTro = await _context.VaiTros.FindAsync(id);
            if (vaiTro == null) return NotFound();

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
            if (!PhanQuyenHelper.IsAdmin(HttpContext.Session))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập vào chức năng này!";
                return RedirectToAction("Index", "Home");
            }

            var vaiTro = await _context.VaiTros.FindAsync(id);
            if (vaiTro == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy vai trò này!";
                return RedirectToAction(nameof(VaiTro));
            }

            // Kiểm tra có user nào đang dùng vai trò này không
            var userCount = await _context.NguoiDungs.CountAsync(u => u.IdvaiTro == id && (u.DaXoa == null || u.DaXoa == false));
            if (userCount > 0)
            {
                TempData["ErrorMessage"] = $"Không thể xóa vai trò này vì có {userCount} người dùng đang sử dụng.";
                return RedirectToAction(nameof(VaiTro));
            }

            // Soft delete
            vaiTro.DaXoa = true;
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
                .Where(v => v.DaXoa == null || v.DaXoa == false)
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