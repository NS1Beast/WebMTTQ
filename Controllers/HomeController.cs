using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebMTTQ.Models;
using WebMTTQ.Services;

namespace WebMTTQ.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataMTTQContext _context;
        private readonly ISystemSettingsService _settings;

        public HomeController(DataMTTQContext context, ISystemSettingsService settings)
        {
            _context = context;
            _settings = settings;
        }

        public async Task<IActionResult> Index()
        {
            // Kiểm tra bảo trì trang chủ
            if (await MaintenanceHelper.IsHomeUnderMaintenanceAsync(_settings))
            {
                return View("~/Views/Home/UnderConstruction.cshtml");
            }

            // Lấy danh sách Banner đang hoạt động
            var danhSachBanner = await _context.Banners
                .Where(b => b.TrangThai == true)
                .OrderBy(b => b.ThuTu)
                .ToListAsync();

            // Lấy danh sách các mục Trang chủ đang hoạt động
            var sections = await _context.TrangChuMucs
                .Where(s => s.TrangThai == true)
                .OrderBy(s => s.ThuTu)
                .ThenByDescending(s => s.NgayTao)
                .ToListAsync();

            // Load TrangChuTinTuc items cho các mục có quản lý nội dung con
            // (tin-tuc: tối đa 6, hinh-anh/video/van-ban/lien-ket: tối đa 12)
            var sectionNews = new Dictionary<int, List<TrangChuTinTuc>>();
            foreach (var sec in sections.Where(s => s.TrangThai))
            {
                int maxItems = sec.Loai == "tin-tuc" ? 6 : 12;
                var items = await _context.TrangChuTinTucs
                    .Where(t => t.IdTrangChuMuc == sec.Id && t.TrangThai)
                    .OrderBy(t => t.ThuTu)
                    .ThenByDescending(t => t.NgayTao)
                    .Take(maxItems)
                    .ToListAsync();
                sectionNews[sec.Id] = items;
            }

            // Timeline Section
            var timeline = await _context.TimelineSections
                .Include(s => s.Items)
                .FirstOrDefaultAsync();
            if (timeline != null && timeline.IsEnabled)
            {
                timeline.Items = timeline.Items
                    .Where(i => i.IsEnabled)
                    .OrderBy(i => i.SortOrder)
                    .ToList();
            }

            // Featured News (bài viết nổi bật)
            var featuredNews = await _context.BaiViets
                .Include(b => b.IdchuyenMucNavigation)
                .Where(b => b.TrangThai == "DaDang")
                .OrderByDescending(b => b.LaTinNoiBat)
                .ThenByDescending(b => b.NgayXuatBan)
                .Take(5)
                .ToListAsync();

            var model = new HomePageViewModel
            {
                Banners = danhSachBanner,
                Sections = sections,
                SectionNews = sectionNews,
                Timeline = timeline,
                FeaturedNews = featuredNews
            };

            return View(model);
        }

        public IActionResult UnderConstruction()
        {
            return View();
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}