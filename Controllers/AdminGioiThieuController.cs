using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WebMTTQ.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace WebMTTQ.Controllers
{
    [Route("admin/gioithieu")]
    [KiemTraQuyen(ModuleQuyen.TrangChu)]
    public class AdminGioiThieuController : BaseAdminController
    {
        private readonly DataMTTQContext _context;
        private readonly IWebHostEnvironment _env;

        public AdminGioiThieuController(DataMTTQContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ============================================================
        // QUẢN LÝ NỘI DUNG GIỚI THIỆU CHUNG (Slogan & Nội dung)
        // ============================================================

        [Route("")]
        public async Task<IActionResult> Index()
        {
            var list = await _context.GioiThieuChungs.OrderByDescending(x => x.Id).ToListAsync();
            return View("~/Views/Admin/GioiThieu/Index.cshtml", list);
        }

        [Route("Create")]
        [HttpGet]
        public IActionResult Create() => View("~/Views/Admin/GioiThieu/Create.cshtml");

        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GioiThieuChung model)
        {
            if (ModelState.IsValid)
            {
                // Tắt các trạng thái hiển thị của những cái cũ (Chỉ giữ 1 cái hiển thị)
                if (model.TrangThai == true)
                {
                    var oldActives = await _context.GioiThieuChungs.Where(x => x.TrangThai == true).ToListAsync();
                    oldActives.ForEach(x => x.TrangThai = false);
                }

                _context.GioiThieuChungs.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm phần giới thiệu thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Admin/GioiThieu/Create.cshtml", model);
        }

        [Route("Edit/{id}")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.GioiThieuChungs.FindAsync(id);
            if (item == null) return NotFound();
            return View("~/Views/Admin/GioiThieu/Edit.cshtml", item);
        }

        [Route("Edit/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GioiThieuChung model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                if (model.TrangThai == true)
                {
                    var oldActives = await _context.GioiThieuChungs.Where(x => x.TrangThai == true && x.Id != id).ToListAsync();
                    oldActives.ForEach(x => x.TrangThai = false);
                }

                _context.Update(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Admin/GioiThieu/Edit.cshtml", model);
        }

        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.GioiThieuChungs.FindAsync(id);
            if (item != null)
            {
                _context.GioiThieuChungs.Remove(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Toggle/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id)
        {
            var item = await _context.GioiThieuChungs.FindAsync(id);
            if (item != null)
            {
                var oldActives = await _context.GioiThieuChungs.Where(x => x.TrangThai == true && x.Id != id).ToListAsync();
                oldActives.ForEach(x => x.TrangThai = false);

                item.TrangThai = !item.TrangThai;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã thay đổi hiển thị!";
            }
            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // QUẢN LÝ SECTION NỘI DUNG TRANG GIỚI THIỆU
        // ============================================================

        // GET: /admin/gioithieu/sections
        [Route("sections")]
        public async Task<IActionResult> Sections()
        {
            var sections = await _context.GioiThieuSections
                .OrderBy(x => x.ThuTu)
                .ThenBy(x => x.Id)
                .ToListAsync();
            return View("~/Views/Admin/GioiThieu/Sections.cshtml", sections);
        }

        // GET: /admin/gioithieu/sections/create
        [Route("sections/create")]
        [HttpGet]
        public IActionResult SectionCreate() => View("~/Views/Admin/GioiThieu/SectionCreate.cshtml");

        // POST: /admin/gioithieu/sections/create
        [Route("sections/create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SectionCreate(GioiThieuSection model, IFormFile? FileAnh)
        {
            if (string.IsNullOrWhiteSpace(model.TieuDe))
            {
                ModelState.AddModelError("TieuDe", "Tiêu đề section không được để trống.");
                return View("~/Views/Admin/GioiThieu/SectionCreate.cshtml", model);
            }

            // Xử lý upload ảnh
            if (FileAnh != null && FileAnh.Length > 0)
            {
                var uploadResult = await UploadSectionImageAsync(FileAnh);
                if (!uploadResult.Success)
                {
                    ModelState.AddModelError("HinhAnh", uploadResult.ErrorMessage ?? "Đã có lỗi xảy ra khi tải ảnh lên.");
                    return View("~/Views/Admin/GioiThieu/SectionCreate.cshtml", model);
                }
                model.HinhAnh = uploadResult.Url;
            }

            // Tự động gán thứ tự
            var maxOrder = await _context.GioiThieuSections.MaxAsync(x => (int?)x.ThuTu) ?? 0;
            model.ThuTu = maxOrder + 1;
            model.TrangThai = true;

            _context.GioiThieuSections.Add(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Thêm section thành công!";
            return RedirectToAction(nameof(Sections));
        }

        // GET: /admin/gioithieu/sections/edit/5
        [Route("sections/edit/{id}")]
        [HttpGet]
        public async Task<IActionResult> SectionEdit(int id)
        {
            var item = await _context.GioiThieuSections.FindAsync(id);
            if (item == null) return NotFound();
            return View("~/Views/Admin/GioiThieu/SectionEdit.cshtml", item);
        }

        // POST: /admin/gioithieu/sections/edit/5
        [Route("sections/edit/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SectionEdit(int id, GioiThieuSection model, IFormFile? FileAnh)
        {
            if (id != model.Id) return NotFound();

            if (string.IsNullOrWhiteSpace(model.TieuDe))
            {
                ModelState.AddModelError("TieuDe", "Tiêu đề section không được để trống.");
                return View("~/Views/Admin/GioiThieu/SectionEdit.cshtml", model);
            }

            var existing = await _context.GioiThieuSections.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (existing == null) return NotFound();

            // Xử lý upload ảnh mới
            if (FileAnh != null && FileAnh.Length > 0)
            {
                var uploadResult = await UploadSectionImageAsync(FileAnh);
                if (!uploadResult.Success)
                {
                    ModelState.AddModelError("HinhAnh", uploadResult.ErrorMessage ?? "Đã có lỗi xảy ra khi tải ảnh lên.");
                    return View("~/Views/Admin/GioiThieu/SectionEdit.cshtml", model);
                }
                model.HinhAnh = uploadResult.Url;
            }
            else
            {
                model.HinhAnh = existing.HinhAnh;
            }

            model.ThuTu = existing.ThuTu;
            model.TrangThai = existing.TrangThai;

            _context.Update(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật section thành công!";
            return RedirectToAction(nameof(Sections));
        }

        // POST: /admin/gioithieu/sections/delete/5
        [HttpPost("sections/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SectionDelete(int id)
        {
            var item = await _context.GioiThieuSections.FindAsync(id);
            if (item != null)
            {
                _context.GioiThieuSections.Remove(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa section!";
            }
            return RedirectToAction(nameof(Sections));
        }

        // POST: /admin/gioithieu/sections/toggle/5
        [HttpPost("sections/toggle/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SectionToggle(int id)
        {
            var item = await _context.GioiThieuSections.FindAsync(id);
            if (item != null)
            {
                item.TrangThai = !item.TrangThai;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = item.TrangThai ? "Đã hiển thị section" : "Đã ẩn section";
            }
            return RedirectToAction(nameof(Sections));
        }

        // POST: /admin/gioithieu/sections/moveup/5
        [HttpPost("sections/moveup/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SectionMoveUp(int id)
        {
            var item = await _context.GioiThieuSections.FindAsync(id);
            if (item == null) return RedirectToAction(nameof(Sections));

            var prev = await _context.GioiThieuSections
                .Where(x => x.ThuTu < item.ThuTu)
                .OrderByDescending(x => x.ThuTu)
                .FirstOrDefaultAsync();

            if (prev != null)
            {
                (item.ThuTu, prev.ThuTu) = (prev.ThuTu, item.ThuTu);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Sections));
        }

        // POST: /admin/gioithieu/sections/movedown/5
        [HttpPost("sections/movedown/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SectionMoveDown(int id)
        {
            var item = await _context.GioiThieuSections.FindAsync(id);
            if (item == null) return RedirectToAction(nameof(Sections));

            var next = await _context.GioiThieuSections
                .Where(x => x.ThuTu > item.ThuTu)
                .OrderBy(x => x.ThuTu)
                .FirstOrDefaultAsync();

            if (next != null)
            {
                (item.ThuTu, next.ThuTu) = (next.ThuTu, item.ThuTu);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Sections));
        }

        // POST: /admin/gioithieu/uploadimage
        // AJAX endpoint để upload ảnh chèn vào nội dung section (rich text)
        [HttpPost("uploadimage")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "Vui lòng chọn ảnh." });
            }

            if (file.Length > 10 * 1024 * 1024)
            {
                return Json(new { success = false, message = "Kích thước ảnh không được vượt quá 10MB." });
            }

            if (!WebMTTQ.Services.FileUploadValidator.IsValidImage(file, out var ext) || ext == null)
            {
                return Json(new { success = false, message = "Định dạng ảnh không hợp lệ. Chỉ chấp nhận JPG, PNG, GIF, WEBP, BMP." });
            }

            try
            {
                string webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string uploadsFolder = Path.Combine(webRoot, "uploads", "gioithieu", "content");
                Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = $"{Guid.NewGuid()}{ext}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                await System.IO.File.WriteAllBytesAsync(filePath, ms.ToArray());

                string url = "/uploads/gioithieu/content/" + uniqueFileName;
                return Json(new { success = true, url = url });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi tải ảnh lên: {ex.Message}" });
            }
        }

        // Helper: upload ảnh đại diện section
        private async Task<(bool Success, string? Url, string? ErrorMessage)> UploadSectionImageAsync(IFormFile file)
        {
            if (file.Length > 5 * 1024 * 1024)
            {
                return (false, null, "Kích thước ảnh không được vượt quá 5MB.");
            }

            if (!WebMTTQ.Services.FileUploadValidator.IsValidImage(file, out var ext) || ext == null)
            {
                return (false, null, "Định dạng ảnh không hợp lệ. Chỉ chấp nhận JPG, PNG, GIF, WEBP, BMP.");
            }

            try
            {
                string webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string uploadsFolder = Path.Combine(webRoot, "uploads", "gioithieu", "sections");
                Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = $"{Guid.NewGuid()}{ext}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                await System.IO.File.WriteAllBytesAsync(filePath, ms.ToArray());

                return (true, "/uploads/gioithieu/sections/" + uniqueFileName, null);
            }
            catch (Exception ex)
            {
                return (false, null, $"Lỗi khi tải ảnh lên: {ex.Message}");
            }
        }
    }
}
