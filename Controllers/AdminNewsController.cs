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
    [KiemTraQuyen(ModuleQuyen.TinTuc)]
    public class AdminNewsController : BaseAdminController
    {
        private readonly DataMTTQContext _context;
        private readonly IWebHostEnvironment _env;

        public AdminNewsController(DataMTTQContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ============================================================
        // QUẢN LÝ BÀI VIẾT
        // ============================================================

        // GET: /AdminNews/Index
        public async Task<IActionResult> Index(string? category = null, string? keyword = null, int page = 1)
        {
            const int pageSize = 10;

            var categories = await _context.ChuyenMucs
                .Where(c => c.LoaiChuyenMuc == LoaiChuyenMucConstants.TinTuc)
                .OrderBy(c => c.ThuTu)
                .ToListAsync();

            IQueryable<BaiViet> query = _context.BaiViets
                .Include(b => b.IdchuyenMucNavigation);

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(b => b.IdchuyenMucNavigation != null && b.IdchuyenMucNavigation.DuongDan == category);
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(b => b.TieuDe.Contains(keyword) || (b.TomTat != null && b.TomTat.Contains(keyword)));
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (totalPages == 0) totalPages = 1;
            if (page > totalPages) page = totalPages;

            var articles = await query
                .OrderByDescending(b => b.NgayXuatBan)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = category;
            ViewBag.Keyword = keyword;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View("~/Views/Admin/News/Index.cshtml", articles);
        }

        // GET: /AdminNews/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _context.ChuyenMucs
                .Where(c => c.LoaiChuyenMuc == LoaiChuyenMucConstants.TinTuc)
                .OrderBy(c => c.ThuTu)
                .ToListAsync();
            ViewBag.Categories = categories;
            return View("~/Views/Admin/News/Create.cshtml");
        }

        // POST: /AdminNews/UploadImage
        // AJAX endpoint để upload ảnh chèn vào nội dung bài viết
        [HttpPost]
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
                string uploadsFolder = Path.Combine(webRoot, "uploads", "news", "content");
                Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = $"{Guid.NewGuid()}{ext}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                await System.IO.File.WriteAllBytesAsync(filePath, ms.ToArray());

                string url = "/uploads/news/content/" + uniqueFileName;
                return Json(new { success = true, url = url });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi tải ảnh lên: {ex.Message}" });
            }
        }

        // POST: /AdminNews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BaiViet baiViet, IFormFile? FileAnh)
        {
            ModelState.Remove("DuongDan");
            ModelState.Remove("AnhDaiDien");
            ModelState.Remove("HinhAnh");
            ModelState.Remove("VideoUrl");
            ModelState.Remove("NgayXuatBan");
            ModelState.Remove("LuotXem");
            ModelState.Remove("LaTinNoiBat");
            ModelState.Remove("IdnguoiDung");

            if (ModelState.IsValid)
            {
                // Tạo đường dẫn từ tiêu đề
                baiViet.DuongDan = GenerateSlug(baiViet.TieuDe);
                baiViet.NgayXuatBan = DateTime.Now;
                baiViet.LuotXem = 0;
                baiViet.TrangThai = string.IsNullOrEmpty(baiViet.TrangThai) ? "DaDang" : baiViet.TrangThai;

                // Lấy ID người dùng hiện tại từ session
                var userIdStr = HttpContext.Session.GetString("AdminUserId");
                if (int.TryParse(userIdStr, out int userId))
                {
                    baiViet.IdnguoiDung = userId;
                }

                // Xử lý upload ảnh
                if (FileAnh != null && FileAnh.Length > 0)
                {
                    if (FileAnh.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("HinhAnh", "Kích thước ảnh không được vượt quá 5MB.");
                        var cats = await _context.ChuyenMucs.Where(c => c.LoaiChuyenMuc == LoaiChuyenMucConstants.TinTuc).ToListAsync();
                        ViewBag.Categories = cats;
                        return View("~/Views/Admin/News/Create.cshtml", baiViet);
                    }

                    try
                    {
                        if (!WebMTTQ.Services.FileUploadValidator.IsValidImage(FileAnh, out var ext) || ext == null)
                        {
                            ModelState.AddModelError("HinhAnh", "Định dạng ảnh không hợp lệ.");
                            var cats = await _context.ChuyenMucs.Where(c => c.LoaiChuyenMuc == LoaiChuyenMucConstants.TinTuc).ToListAsync();
                            ViewBag.Categories = cats;
                            return View("~/Views/Admin/News/Create.cshtml", baiViet);
                        }

                        string webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                        string uploadsFolder = Path.Combine(webRoot, "uploads", "news");
                        Directory.CreateDirectory(uploadsFolder);

                        string uniqueFileName = $"{Guid.NewGuid()}{ext}";
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using var ms = new MemoryStream();
                        await FileAnh.CopyToAsync(ms);
                        await System.IO.File.WriteAllBytesAsync(filePath, ms.ToArray());

                        baiViet.HinhAnh = "/uploads/news/" + uniqueFileName;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("HinhAnh", $"Lỗi khi tải ảnh lên: {ex.Message}");
                        var cats = await _context.ChuyenMucs.Where(c => c.LoaiChuyenMuc == LoaiChuyenMucConstants.TinTuc).ToListAsync();
                        ViewBag.Categories = cats;
                        return View("~/Views/Admin/News/Create.cshtml", baiViet);
                    }
                }

                _context.BaiViets.Add(baiViet);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm bài viết thành công!";
                return RedirectToAction(nameof(Index));
            }

            var categories = await _context.ChuyenMucs.Where(c => c.LoaiChuyenMuc == LoaiChuyenMucConstants.TinTuc).ToListAsync();
            ViewBag.Categories = categories;
            return View("~/Views/Admin/News/Create.cshtml", baiViet);
        }

        // GET: /AdminNews/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var baiViet = await _context.BaiViets
                .Include(b => b.IdchuyenMucNavigation)
                .FirstOrDefaultAsync(b => b.IdbaiViet == id);
            if (baiViet == null) return NotFound();

            var categories = await _context.ChuyenMucs
                .Where(c => c.LoaiChuyenMuc == LoaiChuyenMucConstants.TinTuc)
                .OrderBy(c => c.ThuTu)
                .ToListAsync();
            ViewBag.Categories = categories;
            return View("~/Views/Admin/News/Edit.cshtml", baiViet);
        }

        // POST: /AdminNews/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BaiViet baiViet, IFormFile? FileAnh)
        {
            if (id != baiViet.IdbaiViet) return NotFound();

            var existing = await _context.BaiViets.FindAsync(id);
            if (existing == null) return NotFound();

            ModelState.Remove("DuongDan");
            ModelState.Remove("AnhDaiDien");
            ModelState.Remove("HinhAnh");
            ModelState.Remove("VideoUrl");
            ModelState.Remove("NgayXuatBan");
            ModelState.Remove("LuotXem");
            ModelState.Remove("LaTinNoiBat");
            ModelState.Remove("IdnguoiDung");

            if (ModelState.IsValid)
            {
                existing.TieuDe = baiViet.TieuDe;
                existing.TomTat = baiViet.TomTat;
                existing.NoiDung = baiViet.NoiDung;
                existing.VideoUrl = baiViet.VideoUrl;
                existing.IdchuyenMuc = baiViet.IdchuyenMuc;
                existing.TrangThai = baiViet.TrangThai;
                existing.LaTinNoiBat = baiViet.LaTinNoiBat;
                existing.DuongDan = GenerateSlug(baiViet.TieuDe);

                if (FileAnh != null && FileAnh.Length > 0)
                {
                    if (FileAnh.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("HinhAnh", "Kích thước ảnh không được vượt quá 5MB.");
                        var cats = await _context.ChuyenMucs.Where(c => c.LoaiChuyenMuc == LoaiChuyenMucConstants.TinTuc).ToListAsync();
                        ViewBag.Categories = cats;
                        return View("~/Views/Admin/News/Edit.cshtml", existing);
                    }

                    try
                    {
                        if (!WebMTTQ.Services.FileUploadValidator.IsValidImage(FileAnh, out var ext) || ext == null)
                        {
                            ModelState.AddModelError("HinhAnh", "Định dạng ảnh không hợp lệ.");
                            var cats = await _context.ChuyenMucs.Where(c => c.LoaiChuyenMuc == LoaiChuyenMucConstants.TinTuc).ToListAsync();
                            ViewBag.Categories = cats;
                            return View("~/Views/Admin/News/Edit.cshtml", existing);
                        }

                        string webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                        string uploadsFolder = Path.Combine(webRoot, "uploads", "news");
                        Directory.CreateDirectory(uploadsFolder);

                        string uniqueFileName = $"{Guid.NewGuid()}{ext}";
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using var ms = new MemoryStream();
                        await FileAnh.CopyToAsync(ms);
                        await System.IO.File.WriteAllBytesAsync(filePath, ms.ToArray());

                        existing.HinhAnh = "/uploads/news/" + uniqueFileName;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("HinhAnh", $"Lỗi khi tải ảnh lên: {ex.Message}");
                        var cats = await _context.ChuyenMucs.Where(c => c.LoaiChuyenMuc == LoaiChuyenMucConstants.TinTuc).ToListAsync();
                        ViewBag.Categories = cats;
                        return View("~/Views/Admin/News/Edit.cshtml", existing);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật bài viết thành công!";
                return RedirectToAction(nameof(Index));
            }

            var categories = await _context.ChuyenMucs.Where(c => c.LoaiChuyenMuc == LoaiChuyenMucConstants.TinTuc).ToListAsync();
            ViewBag.Categories = categories;
            return View("~/Views/Admin/News/Edit.cshtml", existing);
        }

        // POST: /AdminNews/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var baiViet = await _context.BaiViets.FindAsync(id);
            if (baiViet != null)
            {
                _context.BaiViets.Remove(baiViet);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa bài viết thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: /AdminNews/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var baiViet = await _context.BaiViets.FindAsync(id);
            if (baiViet != null)
            {
                baiViet.TrangThai = baiViet.TrangThai == "DaDang" ? "An" : "DaDang";
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = baiViet.TrangThai == "DaDang" ? "Đã hiển thị bài viết" : "Đã ẩn bài viết";
            }
            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // QUẢN LÝ CHUYÊN MỤC
        // ============================================================

        // GET: /AdminNews/Categories
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.ChuyenMucs
                .Where(c => c.LoaiChuyenMuc == LoaiChuyenMucConstants.TinTuc)
                .OrderBy(c => c.ThuTu)
                .ThenBy(c => c.TenChuyenMuc)
                .ToListAsync();

            // Đếm số bài viết cho mỗi chuyên mục
            var articleCounts = await _context.BaiViets
                .GroupBy(b => b.IdchuyenMuc)
                .Select(g => new { Id = g.Key ?? 0, Count = g.Count() })
                .ToDictionaryAsync(x => x.Id, x => x.Count);

            ViewBag.ArticleCounts = articleCounts;
            return View("~/Views/Admin/News/Categories.cshtml", categories);
        }

        // GET: /AdminNews/CategoryCreate
        [HttpGet]
        public IActionResult CategoryCreate()
        {
            return View("~/Views/Admin/News/CategoryCreate.cshtml");
        }

        // POST: /AdminNews/CategoryCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoryCreate(ChuyenMuc chuyenMuc)
        {
            ModelState.Remove("DuongDan");
            ModelState.Remove("HienThi");

            if (ModelState.IsValid)
            {
                chuyenMuc.DuongDan = GenerateSlug(chuyenMuc.TenChuyenMuc);
                chuyenMuc.HienThi = true;
                chuyenMuc.ThuTu = chuyenMuc.ThuTu ?? 0;
                chuyenMuc.LoaiChuyenMuc = LoaiChuyenMucConstants.TinTuc;

                // Kiểm tra trùng đường dẫn
                var exists = await _context.ChuyenMucs
                    .AnyAsync(c => c.DuongDan == chuyenMuc.DuongDan);
                if (exists)
                {
                    chuyenMuc.DuongDan = chuyenMuc.DuongDan + "-" + Guid.NewGuid().ToString("N").Substring(0, 6);
                }

                _context.ChuyenMucs.Add(chuyenMuc);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm chuyên mục thành công!";
                return RedirectToAction(nameof(Categories));
            }
            return View("~/Views/Admin/News/CategoryCreate.cshtml", chuyenMuc);
        }

        // GET: /AdminNews/CategoryEdit/5
        [HttpGet]
        public async Task<IActionResult> CategoryEdit(int id)
        {
            var chuyenMuc = await _context.ChuyenMucs.FindAsync(id);
            if (chuyenMuc == null) return NotFound();
            return View("~/Views/Admin/News/CategoryEdit.cshtml", chuyenMuc);
        }

        // POST: /AdminNews/CategoryEdit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoryEdit(int id, ChuyenMuc chuyenMuc)
        {
            if (id != chuyenMuc.IdchuyenMuc) return NotFound();

            var existing = await _context.ChuyenMucs.FindAsync(id);
            if (existing == null) return NotFound();

            ModelState.Remove("DuongDan");

            if (ModelState.IsValid)
            {
                existing.TenChuyenMuc = chuyenMuc.TenChuyenMuc;
                existing.DuongDan = GenerateSlug(chuyenMuc.TenChuyenMuc);
                existing.ThuTu = chuyenMuc.ThuTu ?? 0;
                existing.HienThi = chuyenMuc.HienThi ?? true;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật chuyên mục thành công!";
                return RedirectToAction(nameof(Categories));
            }
            return View("~/Views/Admin/News/CategoryEdit.cshtml", existing);
        }

        // POST: /AdminNews/CategoryDelete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoryDelete(int id)
        {
            var chuyenMuc = await _context.ChuyenMucs.FindAsync(id);
            if (chuyenMuc != null)
            {
                // Kiểm tra có bài viết không
                var hasArticles = await _context.BaiViets
                    .AnyAsync(b => b.IdchuyenMuc == id);
                if (hasArticles)
                {
                    TempData["ErrorMessage"] = $"Không thể xóa chuyên mục \"{chuyenMuc.TenChuyenMuc}\" vì vẫn còn bài viết trong chuyên mục này! Vui lòng chuyển hoặc xóa bài viết trước.";
                    return RedirectToAction(nameof(Categories));
                }

                _context.ChuyenMucs.Remove(chuyenMuc);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa chuyên mục thành công!";
            }
            return RedirectToAction(nameof(Categories));
        }

        // POST: /AdminNews/MoveCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveCategory(int id, int newCategoryId)
        {
            var baiViet = await _context.BaiViets.FindAsync(id);
            if (baiViet == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài viết cần chuyển!";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra chuyên mục mới tồn tại
            var newCategory = await _context.ChuyenMucs
                .FirstOrDefaultAsync(c => c.IdchuyenMuc == newCategoryId
                    && c.LoaiChuyenMuc == LoaiChuyenMucConstants.TinTuc);
            if (newCategory == null)
            {
                TempData["ErrorMessage"] = "Chuyên mục đích không tồn tại!";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra chuyên mục mới khác chuyên mục hiện tại
            if (baiViet.IdchuyenMuc == newCategoryId)
            {
                TempData["ErrorMessage"] = "Bài viết đang ở trong chuyên mục này rồi!";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra có ít nhất 2 chuyên mục
            var totalCategories = await _context.ChuyenMucs
                .CountAsync(c => c.LoaiChuyenMuc == LoaiChuyenMucConstants.TinTuc);
            if (totalCategories < 2)
            {
                TempData["ErrorMessage"] = "Cần ít nhất 2 chuyên mục để chuyển bài viết qua lại!";
                return RedirectToAction(nameof(Index));
            }

            baiViet.IdchuyenMuc = newCategoryId;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã chuyển bài viết \"{baiViet.TieuDe}\" sang chuyên mục \"{newCategory.TenChuyenMuc}\" thành công!";
            return RedirectToAction(nameof(Index));
        }

        // POST: /AdminNews/CategoryToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoryToggleStatus(int id)
        {
            var chuyenMuc = await _context.ChuyenMucs.FindAsync(id);
            if (chuyenMuc != null)
            {
                chuyenMuc.HienThi = !(chuyenMuc.HienThi ?? true);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = (chuyenMuc.HienThi ?? true) ? "Đã hiển thị chuyên mục" : "Đã ẩn chuyên mục";
            }
            return RedirectToAction(nameof(Categories));
        }

        // ============================================================
        // HELPER
        // ============================================================

        private string GenerateSlug(string text)
        {
            if (string.IsNullOrEmpty(text)) return "bai-viet-" + Guid.NewGuid().ToString("N").Substring(0, 8);

            // Chuyển tiếng Việt có dấu thành không dấu
            string[] vietnameseChars = new string[]
            {
                "aAeEoOuUiIdDyY",
                "áàạảãâấầậẩẫăắằặẳẵ",
                "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
                "éèẹẻẽêếềệểễ",
                "ÉÈẸẺẼÊẾỀỆỂỄ",
                "óòọỏõôốồộổỗơớờợởỡ",
                "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
                "úùụủũưứừựửữ",
                "ÚÙỤỦŨƯỨỪỰỬỮ",
                "íìịỉĩ",
                "ÍÌỊỈĨ",
                "đ",
                "Đ",
                "ýỳỵỷỹ",
                "ÝỲỴỶỸ"
            };

            for (int i = 1; i < vietnameseChars.Length; i++)
            {
                for (int j = 0; j < vietnameseChars[i].Length; j++)
                {
                    text = text.Replace(vietnameseChars[i][j], vietnameseChars[0][i - 1]);
                }
            }

            // Chuyển thành chữ thường, thay khoảng trắng bằng dấu gạch ngang
            text = text.ToLowerInvariant();
            text = System.Text.RegularExpressions.Regex.Replace(text, @"[^a-z0-9\s-]", "");
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", "-").Trim('-');
            text = System.Text.RegularExpressions.Regex.Replace(text, @"-+", "-");

            if (string.IsNullOrEmpty(text)) return "bai-viet-" + Guid.NewGuid().ToString("N").Substring(0, 8);
            return text;
        }
    }
}
