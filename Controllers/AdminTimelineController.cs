using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebMTTQ.Models;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace WebMTTQ.Controllers
{
    [KiemTraQuyen(ModuleQuyen.TrangChu)]
    public class AdminTimelineController : BaseAdminController
    {
        private readonly DataMTTQContext _context;

        public AdminTimelineController(DataMTTQContext context)
        {
            _context = context;
        }

        // GET: /AdminTimeline/Index
        public async Task<IActionResult> Index()
        {
            var section = await _context.TimelineSections
                .Include(s => s.Items)
                .FirstOrDefaultAsync();

            // Tạo mặc định nếu chưa có section
            if (section == null)
            {
                section = new TimelineSection
                {
                    IsEnabled = true,
                    Eyebrow = "CÁC CÔNG TRÌNH SỐ",
                    Title = "Hành trình chuyển đổi số Phường Tân Định"
                };
                _context.TimelineSections.Add(section);
                await _context.SaveChangesAsync();
            }

            section.Items = section.Items.OrderBy(i => i.SortOrder).ToList();
            return View("~/Views/Admin/Timeline/Index.cshtml", section);
        }

        // POST: /AdminTimeline/SaveSettings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSettings(int id, bool IsEnabled, string? Eyebrow, string? Title)
        {
            var section = await _context.TimelineSections.FindAsync(id);
            if (section == null) return NotFound();

            section.IsEnabled = IsEnabled;
            section.Eyebrow = Eyebrow ?? "";
            section.Title = Title ?? "";

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã cập nhật cài đặt Section!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /AdminTimeline/ItemCreate
        [HttpGet]
        public async Task<IActionResult> ItemCreate()
        {
            var section = await _context.TimelineSections.FirstOrDefaultAsync();
            if (section == null) return NotFound();

            ViewBag.SectionId = section.Id;
            return View("~/Views/Admin/Timeline/ItemCreate.cshtml");
        }

        // POST: /AdminTimeline/ItemCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ItemCreate(int sectionId, string TimeLabel, string Title, string? Description)
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
            return RedirectToAction(nameof(Index));
        }

        // GET: /AdminTimeline/ItemEdit/5
        [HttpGet]
        public async Task<IActionResult> ItemEdit(int id)
        {
            var item = await _context.TimelineItems
                .Include(i => i.TimelineSection)
                .FirstOrDefaultAsync(i => i.Id == id);
            if (item == null) return NotFound();

            return View("~/Views/Admin/Timeline/ItemEdit.cshtml", item);
        }

        // POST: /AdminTimeline/ItemEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ItemEdit(int id, string TimeLabel, string Title, string? Description, bool IsEnabled)
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
            return RedirectToAction(nameof(Index));
        }

        // POST: /AdminTimeline/ItemDelete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ItemDelete(int id)
        {
            var item = await _context.TimelineItems.FindAsync(id);
            if (item != null)
            {
                _context.TimelineItems.Remove(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa Timeline Item thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: /AdminTimeline/ItemToggle/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ItemToggle(int id)
        {
            var item = await _context.TimelineItems.FindAsync(id);
            if (item != null)
            {
                item.IsEnabled = !item.IsEnabled;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = item.IsEnabled ? "Đã hiển thị item" : "Đã ẩn item";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: /AdminTimeline/UpdateSort
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSort([FromBody] List<SortItem> items)
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
    }
}
