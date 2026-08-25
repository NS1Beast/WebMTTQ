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
    [KiemTraQuyen(ModuleQuyen.TrangChu)]
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
        public async Task<IActionResult> Create(TrangChuMuc section, IFormFile? FileAnh)
        {
            ModelState.Remove("HinhAnh");
            ModelState.Remove("NgayTao");
            ModelState.Remove("NgayCapNhat");

            if (ModelState.IsValid)
            {
                section.NgayTao = DateTime.Now;
                section.TrangThai = true;

                // Upload ảnh đại diện mục nếu có
                if (FileAnh != null && FileAnh.Length > 0)
                {
                    if (FileAnh.Length > 3 * 1024 * 1024)
                    {
                        ModelState.AddModelError("HinhAnh", "Kích thước ảnh không được vượt quá 3MB.");
                        return View("~/Views/Admin/TrangChu/Create.cshtml", section);
                    }

                    try
                    {
                        if (!WebMTTQ.Services.FileUploadValidator.IsValidImage(FileAnh, out var ext) || ext == null)
                        {
                            ModelState.AddModelError("HinhAnh", "Định dạng ảnh không hợp lệ.");
                            return View("~/Views/Admin/TrangChu/Create.cshtml", section);
                        }

                        string webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                        string uploadsFolder = Path.Combine(webRoot, "uploads", "trangchu");
                        Directory.CreateDirectory(uploadsFolder);

                        string uniqueFileName = $"{Guid.NewGuid()}{ext}";
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using var ms = new MemoryStream();
                        await FileAnh.CopyToAsync(ms);
                        await System.IO.File.WriteAllBytesAsync(filePath, ms.ToArray());

                        section.HinhAnh = "/uploads/trangchu/" + uniqueFileName;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("HinhAnh", $"Lỗi khi tải ảnh lên: {ex.Message}");
                        return View("~/Views/Admin/TrangChu/Create.cshtml", section);
                    }
                }

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
        public async Task<IActionResult> Edit(int id, TrangChuMuc section, IFormFile? FileAnh)
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

                // Upload ảnh đại diện mục nếu có
                if (FileAnh != null && FileAnh.Length > 0)
                {
                    if (FileAnh.Length > 3 * 1024 * 1024)
                    {
                        ModelState.AddModelError("HinhAnh", "Kích thước ảnh không được vượt quá 3MB.");
                        return View("~/Views/Admin/TrangChu/Edit.cshtml", existing);
                    }

                    try
                    {
                        if (!WebMTTQ.Services.FileUploadValidator.IsValidImage(FileAnh, out var ext) || ext == null)
                        {
                            ModelState.AddModelError("HinhAnh", "Định dạng ảnh không hợp lệ.");
                            return View("~/Views/Admin/TrangChu/Edit.cshtml", existing);
                        }

                        string webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                        string uploadsFolder = Path.Combine(webRoot, "uploads", "trangchu");
                        Directory.CreateDirectory(uploadsFolder);

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
                        return View("~/Views/Admin/TrangChu/Edit.cshtml", existing);
                    }
                }

                existing.NgayCapNhat = DateTime.Now;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật mục Trang chủ thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Admin/TrangChu/Edit.cshtml", section);
        }

        // POST: /AdminTrangChu/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var section = await _context.TrangChuMucs.FindAsync(id);
            if (section != null)
            {
                // Kiểm tra có tin tức trong mục này không
                var hasTinTuc = await _context.TrangChuTinTucs
                    .AnyAsync(t => t.IdTrangChuMuc == id);
                if (hasTinTuc)
                {
                    TempData["ErrorMessage"] = $"Không thể xóa mục \"{section.TieuDe}\" vì vẫn còn tin tức trong mục này! Vui lòng chuyển hoặc xóa tin tức trước.";
                    return RedirectToAction(nameof(Index));
                }

                _context.TrangChuMucs.Remove(section);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa mục Trang chủ thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: /AdminTrangChu/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
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
        // BANNER MANAGEMENT (sub-feature of TrangChu settings)
        // ============================================================

        // GET: /AdminTrangChu/Banner
        public async Task<IActionResult> Banner()
        {
            var banners = await _context.Banners.OrderBy(b => b.ThuTu).ToListAsync();
            return View("~/Views/Admin/Banner/Index.cshtml", banners);
        }

        // GET: /AdminTrangChu/BannerCreate
        [HttpGet]
        public IActionResult BannerCreate()
        {
            return View("~/Views/Admin/Banner/Create.cshtml");
        }

        // POST: /AdminTrangChu/BannerCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BannerCreate(Banner banner, IFormFile FileAnh)
        {
            ModelState.Remove("HinhAnh");

            if (ModelState.IsValid)
            {
                if (FileAnh != null && FileAnh.Length > 0)
                {
                    if (FileAnh.Length > 3 * 1024 * 1024)
                    {
                        ModelState.AddModelError("HinhAnh", "Kích thước ảnh không được vượt quá 3MB.");
                        return View("~/Views/Admin/Banner/Create.cshtml", banner);
                    }

                    if (!WebMTTQ.Services.FileUploadValidator.IsValidImage(FileAnh, out var ext) || ext == null)
                    {
                        ModelState.AddModelError("HinhAnh", "Định dạng ảnh không hợp lệ.");
                        return View("~/Views/Admin/Banner/Create.cshtml", banner);
                    }

                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "banners");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + ext;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await FileAnh.CopyToAsync(fileStream);
                    }

                    banner.HinhAnh = "/uploads/banners/" + uniqueFileName;
                }
                else
                {
                    ModelState.AddModelError("HinhAnh", "Vui lòng chọn ảnh Banner.");
                    return View("~/Views/Admin/Banner/Create.cshtml", banner);
                }

                _context.Banners.Add(banner);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm Banner thành công!";
                return RedirectToAction(nameof(Banner));
            }
            return View("~/Views/Admin/Banner/Create.cshtml", banner);
        }

        // GET: /AdminTrangChu/BannerEdit/5
        [HttpGet]
        public async Task<IActionResult> BannerEdit(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null) return NotFound();
            return View("~/Views/Admin/Banner/Edit.cshtml", banner);
        }

        // POST: /AdminTrangChu/BannerEdit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BannerEdit(int id, Banner banner, IFormFile FileAnh)
        {
            if (id != banner.IdBanner) return NotFound();
            ModelState.Remove("HinhAnh");

            if (ModelState.IsValid)
            {
                var existingBanner = await _context.Banners.FindAsync(id);
                if (existingBanner == null) return NotFound();

                existingBanner.LienKet = banner.LienKet;
                existingBanner.ThuTu = banner.ThuTu;
                existingBanner.TrangThai = banner.TrangThai;
                existingBanner.HieuUng = banner.HieuUng;
                existingBanner.TocDo = banner.TocDo;
                existingBanner.ThoiGianDung = banner.ThoiGianDung;
                existingBanner.MauNen = banner.MauNen;

                if (FileAnh != null && FileAnh.Length > 0)
                {
                    if (FileAnh.Length > 3 * 1024 * 1024)
                    {
                        ModelState.AddModelError("HinhAnh", "Kích thước ảnh không được vượt quá 3MB.");
                        return View("~/Views/Admin/Banner/Edit.cshtml", existingBanner);
                    }

                    if (!WebMTTQ.Services.FileUploadValidator.IsValidImage(FileAnh, out var ext) || ext == null)
                    {
                        ModelState.AddModelError("HinhAnh", "Định dạng ảnh không hợp lệ.");
                        return View("~/Views/Admin/Banner/Edit.cshtml", existingBanner);
                    }

                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "banners");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + ext;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await FileAnh.CopyToAsync(fileStream);
                    }

                    existingBanner.HinhAnh = "/uploads/banners/" + uniqueFileName;
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật Banner thành công!";
                return RedirectToAction(nameof(Banner));
            }
            return View("~/Views/Admin/Banner/Edit.cshtml", banner);
        }

        // POST: /AdminTrangChu/BannerDelete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BannerDelete(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner != null)
            {
                _context.Banners.Remove(banner);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa Banner thành công!";
            }
            return RedirectToAction(nameof(Banner));
        }

        // POST: /AdminTrangChu/BannerToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BannerToggleStatus(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner != null)
            {
                banner.TrangThai = !banner.TrangThai;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = banner.TrangThai ? "Đã hiển thị Banner" : "Đã ẩn Banner";
            }
            return RedirectToAction(nameof(Banner));
        }

        // ============================================================
        // TIMELINE MANAGEMENT (sub-feature of TrangChu settings)
        // ============================================================

        // GET: /AdminTrangChu/Timeline
        public async Task<IActionResult> Timeline()
        {
            var section = await _context.TimelineSections
                .Include(s => s.Items)
                .FirstOrDefaultAsync();

            if (section == null)
            {
                section = new TimelineSection
                {
                    IsEnabled = true,
                    Eyebrow = "CÁC CÔNG TRÌNH SỐ",
                    Title = "Hành trình chuyển đổi số"
                };
                _context.TimelineSections.Add(section);
                await _context.SaveChangesAsync();
            }

            section.Items = section.Items.OrderBy(i => i.SortOrder).ToList();
            return View("~/Views/Admin/Timeline/Index.cshtml", section);
        }

        // POST: /AdminTrangChu/TimelineSaveSettings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TimelineSaveSettings(int id, bool IsEnabled, string? Eyebrow, string? Title)
        {
            var section = await _context.TimelineSections.FindAsync(id);
            if (section == null) return NotFound();

            section.IsEnabled = IsEnabled;
            section.Eyebrow = Eyebrow ?? "";
            section.Title = Title ?? "";

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã cập nhật cài đặt Section!";
            return RedirectToAction(nameof(Timeline));
        }

        // GET: /AdminTrangChu/TimelineItemCreate
        [HttpGet]
        public async Task<IActionResult> TimelineItemCreate()
        {
            var section = await _context.TimelineSections.FirstOrDefaultAsync();
            if (section == null) return NotFound();

            ViewBag.SectionId = section.Id;
            return View("~/Views/Admin/Timeline/ItemCreate.cshtml");
        }

        // POST: /AdminTrangChu/TimelineItemCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TimelineItemCreate(int sectionId, string TimeLabel, string Title, string? Description)
        {
            var section = await _context.TimelineSections.FindAsync(sectionId);
            if (section == null) return NotFound();

            if (string.IsNullOrWhiteSpace(TimeLabel) || string.IsNullOrWhiteSpace(Title))
            {
                ModelState.AddModelError("", "TimeLabel và Title là bắt buộc.");
                ViewBag.SectionId = sectionId;
                return View("~/Views/Admin/Timeline/ItemCreate.cshtml");
            }

            var maxOrder = await _context.TimelineItems
                .Where(i => i.IdTimelineSection == sectionId)
                .MaxAsync(i => (int?)i.SortOrder) ?? 0;

            var item = new TimelineItem
            {
                IdTimelineSection = sectionId,
                TimeLabel = TimeLabel.Trim(),
                Title = Title.Trim(),
                Description = Description?.Trim(),
                IsEnabled = true,
                SortOrder = maxOrder + 1
            };

            _context.TimelineItems.Add(item);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Thêm Timeline Item thành công!";
            return RedirectToAction(nameof(Timeline));
        }

        // GET: /AdminTrangChu/TimelineItemEdit/5
        [HttpGet]
        public async Task<IActionResult> TimelineItemEdit(int id)
        {
            var item = await _context.TimelineItems
                .Include(i => i.TimelineSection)
                .FirstOrDefaultAsync(i => i.Id == id);
            if (item == null) return NotFound();

            return View("~/Views/Admin/Timeline/ItemEdit.cshtml", item);
        }

        // POST: /AdminTrangChu/TimelineItemEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TimelineItemEdit(int id, string TimeLabel, string Title, string? Description, bool IsEnabled)
        {
            var item = await _context.TimelineItems.FindAsync(id);
            if (item == null) return NotFound();

            if (string.IsNullOrWhiteSpace(TimeLabel) || string.IsNullOrWhiteSpace(Title))
            {
                ModelState.AddModelError("", "TimeLabel và Title là bắt buộc.");
                return View("~/Views/Admin/Timeline/ItemEdit.cshtml", item);
            }

            item.TimeLabel = TimeLabel.Trim();
            item.Title = Title.Trim();
            item.Description = Description?.Trim();
            item.IsEnabled = IsEnabled;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật Timeline Item thành công!";
            return RedirectToAction(nameof(Timeline));
        }

        // POST: /AdminTrangChu/TimelineItemDelete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TimelineItemDelete(int id)
        {
            var item = await _context.TimelineItems.FindAsync(id);
            if (item != null)
            {
                _context.TimelineItems.Remove(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa Timeline Item thành công!";
            }
            return RedirectToAction(nameof(Timeline));
        }

        // POST: /AdminTrangChu/TimelineItemToggle/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TimelineItemToggle(int id)
        {
            var item = await _context.TimelineItems.FindAsync(id);
            if (item != null)
            {
                item.IsEnabled = !item.IsEnabled;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = item.IsEnabled ? "Đã hiển thị item" : "Đã ẩn item";
            }
            return RedirectToAction(nameof(Timeline));
        }

        // POST: /AdminTrangChu/TimelineItemMoveUp/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TimelineItemMoveUp(int id)
        {
            var item = await _context.TimelineItems.FindAsync(id);
            if (item == null) return NotFound();

            var siblings = await _context.TimelineItems
                .Where(i => i.IdTimelineSection == item.IdTimelineSection)
                .OrderBy(i => i.SortOrder)
                .ToListAsync();

            var currentIndex = siblings.FindIndex(i => i.Id == id);
            if (currentIndex <= 0) return RedirectToAction(nameof(Timeline));

            var previousItem = siblings[currentIndex - 1];
            (previousItem.SortOrder, item.SortOrder) = (item.SortOrder, previousItem.SortOrder);

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã di chuyển mốc thời gian lên!";
            return RedirectToAction(nameof(Timeline));
        }

        // POST: /AdminTrangChu/TimelineItemMoveDown/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TimelineItemMoveDown(int id)
        {
            var item = await _context.TimelineItems.FindAsync(id);
            if (item == null) return NotFound();

            var siblings = await _context.TimelineItems
                .Where(i => i.IdTimelineSection == item.IdTimelineSection)
                .OrderBy(i => i.SortOrder)
                .ToListAsync();

            var currentIndex = siblings.FindIndex(i => i.Id == id);
            if (currentIndex < 0 || currentIndex >= siblings.Count - 1) return RedirectToAction(nameof(Timeline));

            var nextItem = siblings[currentIndex + 1];
            (nextItem.SortOrder, item.SortOrder) = (item.SortOrder, nextItem.SortOrder);

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã di chuyển mốc thời gian xuống!";
            return RedirectToAction(nameof(Timeline));
        }

        // POST: /AdminTrangChu/TimelineUpdateSort
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TimelineUpdateSort([FromBody] List<SortItem> items)
        {
            if (items == null || items.Count == 0) return Json(new { success = false });

            foreach (var si in items)
            {
                var item = await _context.TimelineItems.FindAsync(si.id);
                if (item != null)
                {
                    item.SortOrder = si.order;
                }
            }
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        public class SortItem
        {
            public int id { get; set; }
            public int order { get; set; }
        }

        // ============================================================
        // CRUD for TrangChuTinTuc (individual items within a section)
        // Works for ALL section types: tin-tuc, hinh-anh, video, van-ban, lien-ket
        // ============================================================

        // GET: /AdminTrangChu/Items/5
        public async Task<IActionResult> ItemList(int id)
        {
            var section = await _context.TrangChuMucs.FindAsync(id);
            if (section == null) return NotFound();

            var items = await _context.TrangChuTinTucs
                .Where(t => t.IdTrangChuMuc == id)
                .OrderBy(t => t.ThuTu)
                .ThenByDescending(t => t.NgayTao)
                .ToListAsync();

            ViewBag.Section = section;
            ViewBag.Sections = await _context.TrangChuMucs
                .Where(s => s.Loai == section.Loai)
                .OrderBy(s => s.ThuTu)
                .ToListAsync();
            return View("~/Views/Admin/TrangChu/Items/Index.cshtml", items);
        }

        // POST: /AdminTrangChu/Items/Move/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ItemMove(int id, int newSectionId)
        {
            var item = await _context.TrangChuTinTucs.FindAsync(id);
            if (item == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy nội dung cần chuyển!";
                return RedirectToAction(nameof(Index));
            }

            var currentSection = await _context.TrangChuMucs.FindAsync(item.IdTrangChuMuc);
            if (currentSection == null) return NotFound();

            // Kiểm tra mục đích tồn tại và cùng loại
            var newSection = await _context.TrangChuMucs
                .FirstOrDefaultAsync(s => s.Id == newSectionId && s.Loai == currentSection.Loai);
            if (newSection == null)
            {
                TempData["ErrorMessage"] = "Mục đích không tồn tại hoặc không cùng loại!";
                return RedirectToAction(nameof(ItemList), new { id = item.IdTrangChuMuc });
            }

            // Kiểm tra mục mới khác mục hiện tại
            if (item.IdTrangChuMuc == newSectionId)
            {
                TempData["ErrorMessage"] = "Nội dung đang ở trong mục này rồi!";
                return RedirectToAction(nameof(ItemList), new { id = item.IdTrangChuMuc });
            }

            // Kiểm tra có ít nhất 2 mục cùng loại
            var totalSections = await _context.TrangChuMucs
                .CountAsync(s => s.Loai == currentSection.Loai);
            if (totalSections < 2)
            {
                TempData["ErrorMessage"] = $"Cần ít nhất 2 mục cùng loại để chuyển qua lại!";
                return RedirectToAction(nameof(ItemList), new { id = item.IdTrangChuMuc });
            }

            int oldSectionId = item.IdTrangChuMuc;
            item.IdTrangChuMuc = newSectionId;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã chuyển \"{item.TieuDe}\" sang mục \"{newSection.TieuDe}\" thành công!";
            return RedirectToAction(nameof(ItemList), new { id = oldSectionId });
        }

        // GET: /AdminTrangChu/Items/Create/5
        [HttpGet]
        public async Task<IActionResult> ItemCreate(int id)
        {
            var section = await _context.TrangChuMucs.FindAsync(id);
            if (section == null) return NotFound();
            ViewBag.Section = section;

            // Load danh sách văn bản tài liệu nếu mục là loại van-ban
            if (section.Loai == "van-ban")
            {
                ViewBag.VanBans = await _context.VanBanTaiLieus
                    .OrderByDescending(v => v.NgayBanHanh)
                    .ThenByDescending(v => v.IdvanBan)
                    .ToListAsync();
            }

            return View("~/Views/Admin/TrangChu/Items/Create.cshtml");
        }

        // POST: /AdminTrangChu/Items/Create/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ItemCreate(IFormFile? FileAnh)
        {
            // Read sectionId from form data manually to avoid model binding conflicts
            var sectionIdStr = Request.Form["sectionId"].FirstOrDefault();
            if (!int.TryParse(sectionIdStr, out int sectionId))
                return NotFound();

            var section = await _context.TrangChuMucs.FindAsync(sectionId);
            if (section == null) return NotFound();

            // Create new instance manually - no model binding to avoid Id conflict
            var item = new TrangChuTinTuc
            {
                IdTrangChuMuc = sectionId,
                TieuDe = Request.Form["TieuDe"].FirstOrDefault() ?? "",
                TomTat = Request.Form["TomTat"].FirstOrDefault(),
                LienKet = Request.Form["LienKet"].FirstOrDefault(),
                ThuTu = int.TryParse(Request.Form["ThuTu"].FirstOrDefault(), out var thuTu) ? thuTu : 0,
                TrangThai = true,
                NgayTao = DateTime.Now
            };

            // Nếu mục là loại van-ban, cho phép chọn từ danh sách văn bản tài liệu
            if (section.Loai == "van-ban")
            {
                var vanBanIdStr = Request.Form["VanBanId"].FirstOrDefault();
                if (int.TryParse(vanBanIdStr, out int vanBanId) && vanBanId > 0)
                {
                    var vanBan = await _context.VanBanTaiLieus.FindAsync(vanBanId);
                    if (vanBan != null)
                    {
                        item.TieuDe = vanBan.TenVanBan;
                        item.LienKet = $"/VanBanTaiLieu/Download/{vanBan.IdvanBan}";
                        item.TomTat = !string.IsNullOrEmpty(vanBan.SoHieu)
                            ? $"Số hiệu: {vanBan.SoHieu}" + (vanBan.NgayBanHanh.HasValue ? $" - Ngày ban hành: {vanBan.NgayBanHanh.Value:dd/MM/yyyy}" : "")
                            : (vanBan.NgayBanHanh.HasValue ? $"Ngày ban hành: {vanBan.NgayBanHanh.Value:dd/MM/yyyy}" : "");
                    }
                }
            }

            if (FileAnh != null && FileAnh.Length > 0)
            {
                if (FileAnh.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("HinhAnh", "Kích thước ảnh không được vượt quá 5MB.");
                    ViewBag.Section = section;
                    return View("~/Views/Admin/TrangChu/Items/Create.cshtml", item);
                }

                try
                {
                    if (!WebMTTQ.Services.FileUploadValidator.IsValidImage(FileAnh, out var ext) || ext == null)
                    {
                        ModelState.AddModelError("HinhAnh", "Định dạng ảnh không hợp lệ.");
                        ViewBag.Section = section;
                        return View("~/Views/Admin/TrangChu/Items/Create.cshtml", item);
                    }

                    // Use content root path to ensure we save to wwwroot/uploads
                    string webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    string uploadsFolder = Path.Combine(webRoot, "uploads", "trangchu");
                    Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = $"{Guid.NewGuid()}{ext}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using var ms = new MemoryStream();
                    await FileAnh.CopyToAsync(ms);
                    await System.IO.File.WriteAllBytesAsync(filePath, ms.ToArray());

                    item.HinhAnh = "/uploads/trangchu/" + uniqueFileName;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("HinhAnh", $"Lỗi khi tải ảnh lên: {ex.Message}");
                    ViewBag.Section = section;
                    return View("~/Views/Admin/TrangChu/Items/Create.cshtml", item);
                }
            }

            // Validate: TieuDe required for all types
            if (string.IsNullOrWhiteSpace(item.TieuDe))
            {
                ModelState.AddModelError("TieuDe", "Tiêu đề không được để trống.");
                ViewBag.Section = section;
                return View("~/Views/Admin/TrangChu/Items/Create.cshtml", item);
            }

            // Validate: HinhAnh required for hinh-anh type
            if (section.Loai == "hinh-anh" && string.IsNullOrEmpty(item.HinhAnh))
            {
                ModelState.AddModelError("HinhAnh", "Vui lòng chọn ảnh cho thư viện.");
                ViewBag.Section = section;
                return View("~/Views/Admin/TrangChu/Items/Create.cshtml", item);
            }

            // Validate: LienKet required for video type
            if (section.Loai == "video" && string.IsNullOrWhiteSpace(item.LienKet))
            {
                ModelState.AddModelError("LienKet", "Vui lòng nhập URL video (YouTube, Vimeo...).");
                ViewBag.Section = section;
                return View("~/Views/Admin/TrangChu/Items/Create.cshtml", item);
            }

            _context.TrangChuTinTucs.Add(item);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Thêm nội dung thành công!";
            return RedirectToAction(nameof(ItemList), new { id = sectionId });
        }

        // GET: /AdminTrangChu/Items/Edit/5
        [HttpGet]
        public async Task<IActionResult> ItemEdit(int id)
        {
            var item = await _context.TrangChuTinTucs
                .Include(t => t.TrangChuMuc)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (item == null) return NotFound();

            ViewBag.Section = item.TrangChuMuc;

            // Load danh sách văn bản tài liệu nếu mục là loại van-ban
            if (item.TrangChuMuc?.Loai == "van-ban")
            {
                ViewBag.VanBans = await _context.VanBanTaiLieus
                    .OrderByDescending(v => v.NgayBanHanh)
                    .ThenByDescending(v => v.IdvanBan)
                    .ToListAsync();
            }

            return View("~/Views/Admin/TrangChu/Items/Edit.cshtml", item);
        }

        // POST: /AdminTrangChu/Items/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ItemEdit(int id, TrangChuTinTuc item, IFormFile? FileAnh)
        {
            if (id != item.Id) return NotFound();

            var existing = await _context.TrangChuTinTucs
                .Include(t => t.TrangChuMuc)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (existing == null) return NotFound();

            ModelState.Remove("HinhAnh");
            ModelState.Remove("NgayTao");

            if (ModelState.IsValid)
            {
                existing.TieuDe = item.TieuDe;
                existing.TomTat = item.TomTat;
                existing.LienKet = item.LienKet;
                existing.ThuTu = item.ThuTu;
                existing.TrangThai = item.TrangThai;

                // Nếu mục là loại van-ban, cho phép chọn từ danh sách văn bản tài liệu
                if (existing.TrangChuMuc?.Loai == "van-ban")
                {
                    var vanBanIdStr = Request.Form["VanBanId"].FirstOrDefault();
                    if (int.TryParse(vanBanIdStr, out int vanBanId) && vanBanId > 0)
                    {
                        var vanBan = await _context.VanBanTaiLieus.FindAsync(vanBanId);
                        if (vanBan != null)
                        {
                            existing.TieuDe = vanBan.TenVanBan;
                            existing.LienKet = $"/VanBanTaiLieu/Download/{vanBan.IdvanBan}";
                            existing.TomTat = !string.IsNullOrEmpty(vanBan.SoHieu)
                                ? $"Số hiệu: {vanBan.SoHieu}" + (vanBan.NgayBanHanh.HasValue ? $" - Ngày ban hành: {vanBan.NgayBanHanh.Value:dd/MM/yyyy}" : "")
                                : (vanBan.NgayBanHanh.HasValue ? $"Ngày ban hành: {vanBan.NgayBanHanh.Value:dd/MM/yyyy}" : "");
                        }
                    }
                }

                if (FileAnh != null && FileAnh.Length > 0)
                {
                    if (FileAnh.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("HinhAnh", "Kích thước ảnh không được vượt quá 5MB.");
                        ViewBag.Section = existing.TrangChuMuc;
                        return View("~/Views/Admin/TrangChu/Items/Edit.cshtml", existing);
                    }

                    try
                    {
                        if (!WebMTTQ.Services.FileUploadValidator.IsValidImage(FileAnh, out var ext) || ext == null)
                        {
                            ModelState.AddModelError("HinhAnh", "Định dạng ảnh không hợp lệ.");
                            ViewBag.Section = existing.TrangChuMuc;
                            return View("~/Views/Admin/TrangChu/Items/Edit.cshtml", existing);
                        }

                        string webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                        string uploadsFolder = Path.Combine(webRoot, "uploads", "trangchu");
                        Directory.CreateDirectory(uploadsFolder);

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
                        return View("~/Views/Admin/TrangChu/Items/Edit.cshtml", existing);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật nội dung thành công!";
                return RedirectToAction(nameof(ItemList), new { id = existing.IdTrangChuMuc });
            }
            ViewBag.Section = existing.TrangChuMuc;
            return View("~/Views/Admin/TrangChu/Items/Edit.cshtml", existing);
        }

        // POST: /AdminTrangChu/Items/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ItemDelete(int id)
        {
            var item = await _context.TrangChuTinTucs.FindAsync(id);
            if (item != null)
            {
                int sectionId = item.IdTrangChuMuc;
                _context.TrangChuTinTucs.Remove(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa nội dung thành công!";
                return RedirectToAction(nameof(ItemList), new { id = sectionId });
            }
            TempData["ErrorMessage"] = "Không tìm thấy nội dung này!";
            return RedirectToAction(nameof(Index));
        }

        // POST: /AdminTrangChu/Items/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ItemToggleStatus(int id)
        {
            var item = await _context.TrangChuTinTucs.FindAsync(id);
            if (item != null)
            {
                item.TrangThai = !item.TrangThai;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ItemList), new { id = item?.IdTrangChuMuc ?? 0 });
        }
    }
}
