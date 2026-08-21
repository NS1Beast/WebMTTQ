using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebMTTQ.Models;
using WebMTTQ.Services;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace WebMTTQ.Controllers
{
    public class NewsController : Controller
    {
        private readonly DataMTTQContext _context;
        private readonly ISystemSettingsService _settings;
        private readonly ITruyCapService _truyCapService;

        public NewsController(DataMTTQContext context, ISystemSettingsService settings, ITruyCapService truyCapService)
        {
            _context = context;
            _settings = settings;
            _truyCapService = truyCapService;
        }

        // Trang danh sách tin tức theo chuyên mục
        public async Task<IActionResult> Index(string category = "", int page = 1)
        {
            // Kiểm tra bảo trì trang tin tức
            if (await MaintenanceHelper.IsNewsUnderMaintenanceAsync(_settings))
            {
                return View("~/Views/Home/UnderConstruction.cshtml");
            }

            // Ghi nhận lượt truy cập
            await GhiNhanTruyCapAsync();
            var thongKe = await _truyCapService.LayThongKeAsync();

            const int pageSize = 10;

            // Lấy danh sách chuyên mục TIN TỨC chưa xóa và đang hiển thị - tối đa 10 cho dropdown
            var categories = await _context.ChuyenMucs
                .Where(c => c.HienThi == true && c.LoaiChuyenMuc == LoaiChuyenMucConstants.TinTuc)
                .OrderBy(c => c.ThuTu)
                .ThenBy(c => c.TenChuyenMuc)
                .Take(10)
                .ToListAsync();

            // Bài viết trong chuyên mục
            IQueryable<BaiViet> query = _context.BaiViets
                .Where(b => b.TrangThai == "DaDang");

            int? categoryId = null;
            string currentCategoryName = "Tin tức";

            if (!string.IsNullOrEmpty(category))
            {
                var cat = categories.FirstOrDefault(c => c.DuongDan == category)
                    ?? await _context.ChuyenMucs
                        .FirstOrDefaultAsync(c => c.DuongDan == category && c.LoaiChuyenMuc == LoaiChuyenMucConstants.TinTuc);
                if (cat != null)
                {
                    categoryId = cat.IdchuyenMuc;
                    currentCategoryName = cat.TenChuyenMuc;
                    query = query.Where(b => b.IdchuyenMuc == categoryId);
                }
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

            // Lấy văn bản mới cho sidebar
            var recentDocs = await _context.VanBanTaiLieus
                .OrderByDescending(v => v.NgayBanHanh)
                .Take(5)
                .Select(v => new SidebarDocItem
                {
                    Title = (v.SoHieu ?? "") + " - " + v.TenVanBan,
                    Url = "/VanBanTaiLieu/Details/" + v.IdvanBan
                })
                .ToListAsync();

            var model = new NewsIndexViewModel
            {
                PageTitle = currentCategoryName,
                HeroTitle = "Danh mục: " + currentCategoryName,
                HeroEyebrow = "Chuyên mục",
                CurrentCategory = category,
                CurrentCategoryName = currentCategoryName,
                HasArticles = totalItems > 0,
                Categories = categories.Select(c => new NewsCategoryInfo
                {
                    Slug = c.DuongDan,
                    Title = c.TenChuyenMuc,
                    Url = "/News?category=" + c.DuongDan,
                    IsActive = c.DuongDan == category
                }).ToList(),
                Articles = articles.Select(a => new NewsArticleItem
                {
                    Title = a.TieuDe,
                    Url = "/News/Details/" + a.IdbaiViet,
                    ImageUrl = GetImageUrl(a),
                    Date = a.NgayXuatBan?.ToString("dd/MM/yyyy") ?? "",
                    Excerpt = a.TomTat ?? ""
                }).ToList(),
                Pagination = new NewsPaginationInfo
                {
                    CurrentPage = page,
                    TotalPages = totalPages,
                    BaseUrl = "/News?category=" + category
                },
                RecentDocs = recentDocs,
                ThongKeTruyCap = thongKe
            };

            ViewBag.Title = model.PageTitle;
            return View(model);
        }

        // Chi tiết bài viết
        public async Task<IActionResult> Details(int id)
        {
            // Kiểm tra bảo trì trang tin tức
            if (await MaintenanceHelper.IsNewsUnderMaintenanceAsync(_settings))
            {
                return View("~/Views/Home/UnderConstruction.cshtml");
            }

            // Ghi nhận lượt truy cập
            await GhiNhanTruyCapAsync();
            var thongKe = await _truyCapService.LayThongKeAsync();

            var article = await _context.BaiViets
                .Include(b => b.IdchuyenMucNavigation)
                .Include(b => b.IdnguoiDungNavigation)
                .FirstOrDefaultAsync(b => b.IdbaiViet == id && b.TrangThai == "DaDang");

            if (article == null) return NotFound();

            // Tăng lượt xem
            article.LuotXem = (article.LuotXem ?? 0) + 1;
            await _context.SaveChangesAsync();

            // Lấy danh sách chuyên mục TIN TỨC cho dropdown
            var categories = await _context.ChuyenMucs
                .Where(c => c.HienThi == true && c.LoaiChuyenMuc == LoaiChuyenMucConstants.TinTuc)
                .OrderBy(c => c.ThuTu)
                .Take(10)
                .ToListAsync();

            // Lấy bài viết liên quan (cùng chuyên mục)
            var relatedArticles = await _context.BaiViets
                .Where(b => b.TrangThai == "DaDang"
                    && b.IdchuyenMuc == article.IdchuyenMuc
                    && b.IdbaiViet != article.IdbaiViet)
                .OrderByDescending(b => b.NgayXuatBan)
                .Take(3)
                .ToListAsync();

            var model = new NewsDetailViewModel
            {
                Id = article.IdbaiViet,
                Title = article.TieuDe,
                Excerpt = article.TomTat,
                Content = article.NoiDung,
                ImageUrl = GetImageUrl(article),
                VideoUrl = article.VideoUrl,
                Date = article.NgayXuatBan?.ToString("dd/MM/yyyy") ?? "",
                Author = article.IdnguoiDungNavigation?.HoTen ?? "MTTQ Phường Tân Định",
                ViewCount = article.LuotXem ?? 0,
                CategoryName = article.IdchuyenMucNavigation?.TenChuyenMuc ?? "Tin tức",
                CategorySlug = article.IdchuyenMucNavigation?.DuongDan ?? "",
                RelatedArticles = relatedArticles.Select(a => new NewsArticleItem
                {
                    Title = a.TieuDe,
                    Url = "/News/Details/" + a.IdbaiViet,
                    ImageUrl = GetImageUrl(a),
                    Date = a.NgayXuatBan?.ToString("dd/MM/yyyy") ?? "",
                    Excerpt = a.TomTat ?? ""
                }).ToList(),
                ThongKeTruyCap = thongKe
            };

            ViewBag.Title = model.Title;
            ViewBag.Categories = categories;
            return View(model);
        }

        /// <summary>
        /// Ghi nhận lượt truy cập dựa trên session hiện tại.
        /// </summary>
        private async Task GhiNhanTruyCapAsync()
        {
            // Ghi dữ liệu vào session để đảm bảo session được khởi tạo
            // và giữ nguyên SessionId giữa các request (tránh tạo session mới mỗi lần)
            HttpContext.Session.SetString("Visited", "true");

            var sessionId = HttpContext.Session.Id;
            var duongDan = HttpContext.Request.Path + HttpContext.Request.QueryString;

            await _truyCapService.GhiNhanTruyCapAsync(sessionId, duongDan);
        }

        private string GetImageUrl(BaiViet baiViet)
        {
            if (!string.IsNullOrEmpty(baiViet.HinhAnh))
                return baiViet.HinhAnh;

            // Fallback: nếu có AnhDaiDien (byte[]) thì chuyển thành base64
            if (baiViet.AnhDaiDien != null && baiViet.AnhDaiDien.Length > 0)
            {
                return "data:image/jpeg;base64," + Convert.ToBase64String(baiViet.AnhDaiDien);
            }

            return "/images/UBMTTQ_PhuongTanDinh.jpg";
        }
    }
}