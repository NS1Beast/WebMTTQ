using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebMTTQ.Models;

namespace WebMTTQ.Controllers
{
    [Route("admin")]
    public class AdminController : BaseAdminController
    {
        private readonly DataMTTQContext _context;

        public AdminController(DataMTTQContext context)
        {
            _context = context;
        }

        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            // 1. Thống kê Bài viết (bảng BaiViets)
            try { ViewBag.TongTinTuc = await _context.BaiViets.CountAsync(x => x.DaXoa != true); }
            catch { ViewBag.TongTinTuc = 0; }

            // 2. Thống kê Văn bản tài liệu (bảng VanBanTaiLieus)
            try { ViewBag.TongVanBan = await _context.VanBanTaiLieus.CountAsync(x => x.DaXoa != true); }
            catch
            {
                try { ViewBag.TongVanBan = await _context.VanBanTaiLieus.CountAsync(); }
                catch { ViewBag.TongVanBan = 0; }
            }

            // 3. Thống kê Yêu cầu Trợ giúp
            try
            {
                ViewBag.YeuCauMoi = await _context.NguoiDanCanTroGiups.CountAsync(x => x.DaXoa != true && (x.TrangThai == "Chưa xử lý" || x.TrangThai == null || x.TrangThai == ""));
                ViewBag.YeuCauDaXuLy = await _context.NguoiDanCanTroGiups.CountAsync(x => x.DaXoa != true && (x.TrangThai == "Đã xử lý" || x.TrangThai == "Đã hỗ trợ"));
            }
            catch { ViewBag.YeuCauMoi = 0; ViewBag.YeuCauDaXuLy = 0; }

            // 4. Thống kê Góp ý
            try
            {
                ViewBag.GopYMoi = await _context.HopThuGopies.CountAsync(x => (x.DaXoa == false || x.DaXoa == null) && (x.TrangThai == "Chưa xử lý" || x.TrangThai == null || x.TrangThai == ""));
                ViewBag.GopYDaXuLy = await _context.HopThuGopies.CountAsync(x => (x.DaXoa == false || x.DaXoa == null) && (x.TrangThai == "Đã xử lý" || x.TrangThai == "Đang xử lý"));
            }
            catch { ViewBag.GopYMoi = 0; ViewBag.GopYDaXuLy = 0; }

            // Tổng hợp tương tác (Yêu cầu + Góp ý)
            ViewBag.TongTuongTacMoi = (ViewBag.YeuCauMoi ?? 0) + (ViewBag.GopYMoi ?? 0);
            ViewBag.TongTuongTacDaXuLy = (ViewBag.YeuCauDaXuLy ?? 0) + (ViewBag.GopYDaXuLy ?? 0);

            // 5. Thống kê Số dư 3 Quỹ (Lấy dòng mới nhất của mỗi quỹ)
            try
            {
                var quyNguoiNgheo = await _context.SoDuQuyViNguoiNgheos.OrderByDescending(x => x.NgayCapNhat).FirstOrDefaultAsync();
                var quyBienDao = await _context.SoDuQuyBienDaos.OrderByDescending(x => x.NgayCapNhat).FirstOrDefaultAsync();
                var quyCuuTro = await _context.SoDuQuyCuuTros.OrderByDescending(x => x.NgayCapNhat).FirstOrDefaultAsync();

                decimal tienNguoiNgheo = quyNguoiNgheo?.TongTonQuy ?? 0;
                decimal tienBienDao = quyBienDao?.TongTonQuy ?? 0;
                decimal tienCuuTro = quyCuuTro?.TongTonQuy ?? 0;

                ViewBag.TienNguoiNgheo = tienNguoiNgheo;
                ViewBag.TienBienDao = tienBienDao;
                ViewBag.TienCuuTro = tienCuuTro;
                ViewBag.TongTienCacQuy = tienNguoiNgheo + tienBienDao + tienCuuTro;
            }
            catch
            {
                ViewBag.TienNguoiNgheo = 0; ViewBag.TienBienDao = 0; ViewBag.TienCuuTro = 0; ViewBag.TongTienCacQuy = 0;
            }

            // 6. Lấy 5 yêu cầu trợ giúp MỚI NHẤT cho bảng hiển thị nhanh
            try
            {
                ViewBag.ListYeuCauMoi = await _context.NguoiDanCanTroGiups
                    .Where(x => x.DaXoa != true)
                    .OrderByDescending(x => x.NgayGui)
                    .Take(5)
                    .ToListAsync();
            }
            catch { ViewBag.ListYeuCauMoi = null; }

            return View("~/Views/Admin/Dashboard/Index.cshtml");
        }
    }
}