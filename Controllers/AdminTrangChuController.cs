using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebMTTQ.Models;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace WebMTTQ.Controllers
{
    [KiemTraQuyen(ModuleQuyen.TrangChu, "Xem")]
    public class AdminTrangChuController : BaseAdminController
    {
        private readonly DataMTTQContext _context;
        private readonly IWebHostEnvironment _env;

        public AdminTrangChuController(DataMTTQContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: /AdminTrangChu/Index
        public async Task<IActionResult> Index()
        {
            var sections = await _context.TrangChuMucs
                .Include(s => s.TinTucs)
                .OrderBy(s => s.ThuTu)
                .ThenByDescending(s => s.NgayTao)
                .ToListAsync();
            return View("~/Views/Admin/TrangChu/Index.cshtml", sections);
        }

        // GET: /AdminTrangChu/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View("~/Views/Admin/TrangChu/Create.cshtml");
        }

        // POST: /AdminTrangChu/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrangChuMuc section)
        {
            ModelState.Remove("HinhAnh");
            ModelState.Remove("NgayTao");
            ModelState.Remove("NgayCapNhat");

            if (ModelState.IsValid)
            {
                section.NgayTao = DateTime.Now;
                section.TrangThai = true;

                _context.TrangChuMucs.Add(section);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm mục Trang chủ thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Admin/TrangChu/Create.cshtml", section);
        }

        // GET: /AdminTrangChu/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var section = await _context.TrangChuMucs
                .Include(s => s.TinTucs)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (section == null) return NotFound();
            return View("~/Views/Admin/TrangChu/Edit.cshtml", section);
        }

        // POST: /AdminTrangChu/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TrangChuMuc section)
        {
            if (id != section.Id) return NotFound();

            ModelState.Remove("HinhAnh");
            ModelState.Remove("NgayTao");
            ModelState.Remove("NgayCapNhat");

            if (ModelState.IsValid)
            {
                var existing = await _context.TrangChuMucs.FindAsync(id);
                if (existing == null) return NotFound();

                existing.TieuDe = section.TieuDe;
                existing.Loai = section.Loai;
                existing.NoiDung = section.NoiDung;
                existing.ThuTu = section.ThuTu;
                existing.TrangThai = section.TrangThai;

                existing.NgayCapNhat = DateTime.Now;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật mục Trang chủ thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Admin/TrangChu/Edit.cshtml", section);
        }

        // POST: /AdminTrangChu/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var section = await _context.TrangChuMucs.FindAsync(id);
            if (section != null)
            {
                _context.TrangChuMucs.Remove(section);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa mục Trang chủ thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: /AdminTrangChu/ToggleStatus/5
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var section = await _context.TrangChuMucs.FindAsync(id);
            if (section != null)
            {
                section.TrangThai = !section.TrangThai;
                section.NgayCapNhat = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = section.TrangThai ? "Đã hiển thị mục" : "Đã ẩn mục";
            }
            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // CRUD for TrangChuTinTuc (individual news items within a section)
        // ============================================================

        // GET: /AdminTrangChu/TinTuc/5
        public async Task<IActionResult> TinTucList(int id)
        {
            var section = await _context.TrangChuMucs.FindAsync(id);
            if (section == null) return NotFound();

            var tinTucs = await _context.TrangChuTinTucs
                .Where(t => t.IdTrangChuMuc == id)
                .OrderBy(t => t.ThuTu)
                .ThenByDescending(t => t.NgayTao)
                .ToListAsync();

            ViewBag.Section = section;
            return View("~/Views/Admin/TrangChu/TinTuc/Index.cshtml", tinTucs);
        }

        // GET: /AdminTrangChu/TinTuc/Create/5
        [HttpGet]
        public async Task<IActionResult> TinTucCreate(int id)
        {
            var section = await _context.TrangChuMucs.FindAsync(id);
            if (section == null) return NotFound();
            ViewBag.Section = section;
            return View("~/Views/Admin/TrangChu/TinTuc/Create.cshtml");
        }

        // POST: /AdminTrangChu/TinTuc/Create/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TinTucCreate(IFormFile? FileAnh)
        {
            // Read sectionId from form data manually to avoid model binding conflicts
            var sectionIdStr = Request.Form["sectionId"].FirstOrDefault();
            if (!int.TryParse(sectionIdStr, out int sectionId))
                return NotFound();

            var section = await _context.TrangChuMucs.FindAsync(sectionId);
            if (section == null) return NotFound();

            // Create new instance manually - no model binding to avoid Id conflict
            var tinTuc = new TrangChuTinTuc
            {
                IdTrangChuMuc = sectionId,
                TieuDe = Request.Form["TieuDe"].FirstOrDefault() ?? "",
                TomTat = Request.Form["TomTat"].FirstOrDefault(),
                LienKet = Request.Form["LienKet"].FirstOrDefault(),
                ThuTu = int.TryParse(Request.Form["ThuTu"].FirstOrDefault(), out var thuTu) ? thuTu : 0,
                TrangThai = true,
                NgayTao = DateTime.Now
            };

            if (FileAnh != null && FileAnh.Length > 0)
            {
                if (FileAnh.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("HinhAnh", "Kích thước ảnh không được vượt quá 5MB.");
                    ViewBag.Section = section;
                    return View("~/Views/Admin/TrangChu/TinTuc/Create.cshtml", tinTuc);
                }

                try
                {
                    // Use content root path to ensure we save to wwwroot/uploads
                    string webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    string uploadsFolder = Path.Combine(webRoot, "uploads", "trangchu");
                    Directory.CreateDirectory(uploadsFolder);

                    string ext = Path.GetExtension(FileAnh.FileName);
                    string uniqueFileName = $"{Guid.NewGuid()}{ext}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using var ms = new MemoryStream();
                    await FileAnh.CopyToAsync(ms);
                    await System.IO.File.WriteAllBytesAsync(filePath, ms.ToArray());

                    tinTuc.HinhAnh = "/uploads/trangchu/" + uniqueFileName;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("HinhAnh", $"Lỗi khi tải ảnh lên: {ex.Message}");
                    ViewBag.Section = section;
                    return View("~/Views/Admin/TrangChu/TinTuc/Create.cshtml", tinTuc);
                }
            }

            if (string.IsNullOrWhiteSpace(tinTuc.TieuDe))
            {
                ModelState.AddModelError("TieuDe", "Tiêu đề không được để trống.");
                ViewBag.Section = section;
                return View("~/Views/Admin/TrangChu/TinTuc/Create.cshtml", tinTuc);
            }

            _context.TrangChuTinTucs.Add(tinTuc);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Thêm tin tức thành công!";
            return RedirectToAction(nameof(TinTucList), new { id = sectionId });
        }

        // GET: /AdminTrangChu/TinTuc/Edit/5
        [HttpGet]
        public async Task<IActionResult> TinTucEdit(int id)
        {
            var tinTuc = await _context.TrangChuTinTucs
                .Include(t => t.TrangChuMuc)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (tinTuc == null) return NotFound();

            ViewBag.Section = tinTuc.TrangChuMuc;
            return View("~/Views/Admin/TrangChu/TinTuc/Edit.cshtml", tinTuc);
        }

        // POST: /AdminTrangChu/TinTuc/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TinTucEdit(int id, TrangChuTinTuc tinTuc, IFormFile? FileAnh)
        {
            if (id != tinTuc.Id) return NotFound();

            var existing = await _context.TrangChuTinTucs
                .Include(t => t.TrangChuMuc)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (existing == null) return NotFound();

            ModelState.Remove("HinhAnh");
            ModelState.Remove("NgayTao");

            if (ModelState.IsValid)
            {
                existing.TieuDe = tinTuc.TieuDe;
                existing.TomTat = tinTuc.TomTat;
                existing.LienKet = tinTuc.LienKet;
                existing.ThuTu = tinTuc.ThuTu;
                existing.TrangThai = tinTuc.TrangThai;

                if (FileAnh != null && FileAnh.Length > 0)
                {
                    if (FileAnh.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("HinhAnh", "Kích thước ảnh không được vượt quá 5MB.");
                        ViewBag.Section = existing.TrangChuMuc;
                        return View("~/Views/Admin/TrangChu/TinTuc/Edit.cshtml", existing);
                    }

                    try
                    {
                        string webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                        string uploadsFolder = Path.Combine(webRoot, "uploads", "trangchu");
                        Directory.CreateDirectory(uploadsFolder);

                        string ext = Path.GetExtension(FileAnh.FileName);
                        string uniqueFileName = $"{Guid.NewGuid()}{ext}";
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using var ms = new MemoryStream();
                        await FileAnh.CopyToAsync(ms);
                        await System.IO.File.WriteAllBytesAsync(filePath, ms.ToArray());

                        existing.HinhAnh = "/uploads/trangchu/" + uniqueFileName;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("HinhAnh", $"Lỗi khi tải ảnh lên: {ex.Message}");
                        ViewBag.Section = existing.TrangChuMuc;
                        return View("~/Views/Admin/TrangChu/TinTuc/Edit.cshtml", existing);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật tin tức thành công!";
                return RedirectToAction(nameof(TinTucList), new { id = existing.IdTrangChuMuc });
            }
            ViewBag.Section = existing.TrangChuMuc;
            return View("~/Views/Admin/TrangChu/TinTuc/Edit.cshtml", existing);
        }

        // POST: /AdminTrangChu/TinTuc/Delete/5
        [HttpPost]
        public async Task<IActionResult> TinTucDelete(int id)
        {
            var tinTuc = await _context.TrangChuTinTucs.FindAsync(id);
            if (tinTuc != null)
            {
                int sectionId = tinTuc.IdTrangChuMuc;
                _context.TrangChuTinTucs.Remove(tinTuc);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa tin tức thành công!";
                return RedirectToAction(nameof(TinTucList), new { id = sectionId });
            }
            TempData["ErrorMessage"] = "Không tìm thấy tin tức này!";
            return RedirectToAction(nameof(Index));
        }

        // POST: /AdminTrangChu/TinTuc/ToggleStatus/5
        [HttpPost]
        public async Task<IActionResult> TinTucToggleStatus(int id)
        {
            var tinTuc = await _context.TrangChuTinTucs.FindAsync(id);
            if (tinTuc != null)
            {
                tinTuc.TrangThai = !tinTuc.TrangThai;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(TinTucList), new { id = tinTuc?.IdTrangChuMuc ?? 0 });
        }
    }
}
