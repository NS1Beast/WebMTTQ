using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebMTTQ.Models;

namespace WebMTTQ.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataMTTQContext _context;

        public HomeController(DataMTTQContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
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

            // Load TrangChuTinTuc items cho các mục loại "tin-tuc" (tối đa 6 tin: 1 tin to + 5 tin nhỏ)
            var sectionNews = new Dictionary<int, List<TrangChuTinTuc>>();
            foreach (var sec in sections.Where(s => s.Loai == "tin-tuc" && s.TrangThai))
            {
                var tinTucs = await _context.TrangChuTinTucs
                    .Where(t => t.IdTrangChuMuc == sec.Id && t.TrangThai)
                    .OrderBy(t => t.ThuTu)
                    .ThenByDescending(t => t.NgayTao)
                    .Take(6)
                    .ToListAsync();
                sectionNews[sec.Id] = tinTucs;
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