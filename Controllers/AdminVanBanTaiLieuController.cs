using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebMTTQ.Models;

namespace WebMTTQ.Controllers
{
    // Bọc Route để chống lỗi 404
    [Route("AdminVanBanTaiLieu/[action]/{id?}")]
    [KiemTraQuyen(ModuleQuyen.VanBanTaiLieu)]
    public class AdminVanBanTaiLieuController : BaseAdminController
    {
        private readonly DataMTTQContext _context;

        public AdminVanBanTaiLieuController(DataMTTQContext context)
        {
            _context = context;
        }

        // ============================================================
        // QUẢN LÝ VĂN BẢN TÀI LIỆU
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Quản lý Văn bản & Tài liệu";
            var data = await _context.VanBanTaiLieus
                .Include(v => v.IdchuyenMucNavigation)
                .OrderByDescending(x => x.NgayBanHanh)
                .ThenByDescending(x => x.IdvanBan)
                .ToListAsync();

            // Lấy danh sách chuyên mục để chuyển đổi văn bản
            var categories = await _context.ChuyenMucs
                .Where(c => c.LoaiChuyenMuc == LoaiChuyenMucConstants.VanBanTaiLieu)
                .OrderBy(c => c.ThuTu)
                .ThenBy(c => c.TenChuyenMuc)
                .ToListAsync();

            ViewBag.ChuyenMucs = categories;
            return View("~/Views/Admin/VanBanTaiLieu/Index.cshtml", data);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Title"] = "Thêm Văn bản mới";
            // Chỉ lấy chuyên mục loại "VanBanTaiLieu"
            ViewBag.ChuyenMucs = new SelectList(
                _context.ChuyenMucs.Where(c => c.LoaiChuyenMuc == LoaiChuyenMucConstants.VanBanTaiLieu),
                "IdchuyenMuc", "TenChuyenMuc");
            return View("~/Views/Admin/VanBanTaiLieu/Create.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VanBanTaiLieu model, IFormFile? FileUpload)
        {
            if (FileUpload != null && FileUpload.Length > 0)
            {
                if (!WebMTTQ.Services.FileUploadValidator.IsValidDocument(FileUpload, out var documentExt) || documentExt == null)
                {
                    ModelState.AddModelError("TepDinhKem", "Định dạng file không hợp lệ. Chỉ chấp nhận PDF, DOCX, DOC, XLSX, XLS.");
                    return View("~/Views/Admin/VanBanTaiLieu/Create.cshtml", model);
                }

                using (var ms = new MemoryStream())
                {
                    await FileUpload.CopyToAsync(ms);
                    model.TepDinhKem = ms.ToArray();
                }
                model.LoaiTep = documentExt;
                model.DungLuong = FileUpload.Length;
            }

            _context.VanBanTaiLieus.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã thêm văn bản thành công!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.VanBanTaiLieus.FindAsync(id);
            if (item == null) return NotFound();

            ViewData["Title"] = "Cập nhật Văn bản";
            // Chỉ lấy chuyên mục loại "VanBanTaiLieu"
            ViewBag.ChuyenMucs = new SelectList(
                _context.ChuyenMucs.Where(c => c.LoaiChuyenMuc == LoaiChuyenMucConstants.VanBanTaiLieu),
                "IdchuyenMuc", "TenChuyenMuc", item.IdchuyenMuc);
            return View("~/Views/Admin/VanBanTaiLieu/Edit.cshtml", item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VanBanTaiLieu model, IFormFile? FileUpload)
        {
            var existingItem = await _context.VanBanTaiLieus.AsNoTracking().FirstOrDefaultAsync(x => x.IdvanBan == model.IdvanBan);
            if (existingItem == null) return NotFound();

            if (FileUpload != null && FileUpload.Length > 0)
            {
                if (!WebMTTQ.Services.FileUploadValidator.IsValidDocument(FileUpload, out var documentExt) || documentExt == null)
                {
                    ModelState.AddModelError("TepDinhKem", "Định dạng file không hợp lệ. Chỉ chấp nhận PDF, DOCX, DOC, XLSX, XLS.");
                    return View("~/Views/Admin/VanBanTaiLieu/Edit.cshtml", model);
                }

                using (var ms = new MemoryStream())
                {
                    await FileUpload.CopyToAsync(ms);
                    model.TepDinhKem = ms.ToArray();
                }
                model.LoaiTep = documentExt;
                model.DungLuong = FileUpload.Length;
            }
            else
            {
                model.TepDinhKem = existingItem.TepDinhKem;
                model.LoaiTep = existingItem.LoaiTep;
                model.DungLuong = existingItem.DungLuong;
            }

            _context.VanBanTaiLieus.Update(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật văn bản thành công!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.VanBanTaiLieus.FindAsync(id);
            if (item != null)
            {
                _context.VanBanTaiLieus.Remove(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa văn bản thành công!";
            }
            return RedirectToAction("Index");
        }

        // ============================================================
        // CHUYỂN VĂN BẢN SANG CHUYÊN MỤC KHÁC
        // ============================================================

        // POST: /AdminVanBanTaiLieu/MoveCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveCategory(int id, int newCategoryId)
        {
            var vanBan = await _context.VanBanTaiLieus.FindAsync(id);
            if (vanBan == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy văn bản cần chuyển!";
                return RedirectToAction("Index");
            }

            // Kiểm tra chuyên mục mới tồn tại
            var newCategory = await _context.ChuyenMucs
                .FirstOrDefaultAsync(c => c.IdchuyenMuc == newCategoryId
                    && c.LoaiChuyenMuc == LoaiChuyenMucConstants.VanBanTaiLieu);
            if (newCategory == null)
            {
                TempData["ErrorMessage"] = "Chuyên mục đích không tồn tại!";
                return RedirectToAction("Index");
            }

            // Kiểm tra chuyên mục mới khác chuyên mục hiện tại
            if (vanBan.IdchuyenMuc == newCategoryId)
            {
                TempData["ErrorMessage"] = "Văn bản đang ở trong chuyên mục này rồi!";
                return RedirectToAction("Index");
            }

            // Kiểm tra có ít nhất 2 chuyên mục (đã validate ở client, nhưng check lại server)
            var totalCategories = await _context.ChuyenMucs
                .CountAsync(c => c.LoaiChuyenMuc == LoaiChuyenMucConstants.VanBanTaiLieu);
            if (totalCategories < 2)
            {
                TempData["ErrorMessage"] = "Cần ít nhất 2 chuyên mục để chuyển văn bản qua lại!";
                return RedirectToAction("Index");
            }

            vanBan.IdchuyenMuc = newCategoryId;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã chuyển văn bản \"{vanBan.TenVanBan}\" sang chuyên mục \"{newCategory.TenChuyenMuc}\" thành công!";
            return RedirectToAction("Index");
        }

        // ============================================================
        // QUẢN LÝ CHUYÊN MỤC VĂN BẢN TÀI LIỆU
        // ============================================================

        // GET: /AdminVanBanTaiLieu/Categories
        [HttpGet]
        public async Task<IActionResult> Categories()
        {
            ViewData["Title"] = "Quản lý Chuyên mục Văn bản";
            var categories = await _context.ChuyenMucs
                .Where(c => c.LoaiChuyenMuc == LoaiChuyenMucConstants.VanBanTaiLieu)
                .OrderBy(c => c.ThuTu)
                .ThenBy(c => c.TenChuyenMuc)
                .ToListAsync();

            // Đếm số văn bản cho mỗi chuyên mục
            var docCounts = await _context.VanBanTaiLieus
                .GroupBy(v => v.IdchuyenMuc)
                .Select(g => new { Id = g.Key ?? 0, Count = g.Count() })
                .ToDictionaryAsync(x => x.Id, x => x.Count);

            ViewBag.DocCounts = docCounts;
            return View("~/Views/Admin/VanBanTaiLieu/Categories.cshtml", categories);
        }

        // GET: /AdminVanBanTaiLieu/CategoryCreate
        [HttpGet]
        public IActionResult CategoryCreate()
        {
            ViewData["Title"] = "Thêm Chuyên mục Văn bản";
            return View("~/Views/Admin/VanBanTaiLieu/CategoryCreate.cshtml");
        }

        // POST: /AdminVanBanTaiLieu/CategoryCreate
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
                chuyenMuc.LoaiChuyenMuc = LoaiChuyenMucConstants.VanBanTaiLieu;

                // Kiểm tra trùng đường dẫn
                var exists = await _context.ChuyenMucs
                    .AnyAsync(c => c.DuongDan == chuyenMuc.DuongDan);
                if (exists)
                {
                    chuyenMuc.DuongDan = chuyenMuc.DuongDan + "-" + Guid.NewGuid().ToString("N").Substring(0, 6);
                }

                _context.ChuyenMucs.Add(chuyenMuc);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm chuyên mục văn bản thành công!";
                return RedirectToAction(nameof(Categories));
            }
            return View("~/Views/Admin/VanBanTaiLieu/CategoryCreate.cshtml", chuyenMuc);
        }

        // GET: /AdminVanBanTaiLieu/CategoryEdit/5
        [HttpGet]
        public async Task<IActionResult> CategoryEdit(int id)
        {
            var chuyenMuc = await _context.ChuyenMucs.FindAsync(id);
            if (chuyenMuc == null || chuyenMuc.LoaiChuyenMuc != LoaiChuyenMucConstants.VanBanTaiLieu) return NotFound();
            ViewData["Title"] = "Cập nhật Chuyên mục Văn bản";
            return View("~/Views/Admin/VanBanTaiLieu/CategoryEdit.cshtml", chuyenMuc);
        }

        // POST: /AdminVanBanTaiLieu/CategoryEdit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoryEdit(int id, ChuyenMuc chuyenMuc)
        {
            if (id != chuyenMuc.IdchuyenMuc) return NotFound();

            var existing = await _context.ChuyenMucs.FindAsync(id);
            if (existing == null || existing.LoaiChuyenMuc != LoaiChuyenMucConstants.VanBanTaiLieu) return NotFound();

            ModelState.Remove("DuongDan");

            if (ModelState.IsValid)
            {
                existing.TenChuyenMuc = chuyenMuc.TenChuyenMuc;
                existing.DuongDan = GenerateSlug(chuyenMuc.TenChuyenMuc);
                existing.ThuTu = chuyenMuc.ThuTu ?? 0;
                existing.HienThi = chuyenMuc.HienThi ?? true;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật chuyên mục văn bản thành công!";
                return RedirectToAction(nameof(Categories));
            }
            return View("~/Views/Admin/VanBanTaiLieu/CategoryEdit.cshtml", existing);
        }

        // POST: /AdminVanBanTaiLieu/CategoryDelete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoryDelete(int id)
        {
            var chuyenMuc = await _context.ChuyenMucs.FindAsync(id);
            if (chuyenMuc != null && chuyenMuc.LoaiChuyenMuc == LoaiChuyenMucConstants.VanBanTaiLieu)
            {
                // Kiểm tra có văn bản không
                var hasDocs = await _context.VanBanTaiLieus
                    .AnyAsync(v => v.IdchuyenMuc == id);
                if (hasDocs)
                {
                    TempData["ErrorMessage"] = $"Không thể xóa chuyên mục \"{chuyenMuc.TenChuyenMuc}\" vì vẫn còn văn bản trong chuyên mục này! Vui lòng chuyển hoặc xóa văn bản trước.";
                    return RedirectToAction(nameof(Categories));
                }

                _context.ChuyenMucs.Remove(chuyenMuc);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa chuyên mục văn bản thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy chuyên mục văn bản cần xóa!";
            }
            return RedirectToAction(nameof(Categories));
        }

        // POST: /AdminVanBanTaiLieu/CategoryToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoryToggleStatus(int id)
        {
            var chuyenMuc = await _context.ChuyenMucs.FindAsync(id);
            if (chuyenMuc != null && chuyenMuc.LoaiChuyenMuc == LoaiChuyenMucConstants.VanBanTaiLieu)
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
            if (string.IsNullOrEmpty(text)) return "chuyen-muc-" + Guid.NewGuid().ToString("N").Substring(0, 8);

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
                "íìỉịĩ",
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

            if (string.IsNullOrEmpty(text)) return "chuyen-muc-" + Guid.NewGuid().ToString("N").Substring(0, 8);
            return text;
        }
    }
}
