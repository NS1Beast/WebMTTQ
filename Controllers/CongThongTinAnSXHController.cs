using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebMTTQ.Models;

namespace WebMTTQ.Controllers
{
    public class CongThongTinAnSXHController : Controller
    {
        private readonly DataMTTQContext _context;

        public CongThongTinAnSXHController(DataMTTQContext context)
        {
            _context = context;
        }

        // GET: /CongThongTinAnSXH/
        // Thêm tham số page, mặc định là trang 1
        public async Task<IActionResult> Index(int page = 1)
        {
            // 1. LẤY DỮ LIỆU ĐỔ VÀO THẺ MÃ QR BÊN TRÁI
            ViewBag.DanhSachUngHo = await _context.ThongTinNhanUngHos.ToListAsync();

            // 2. THỐNG KÊ (Tổng số lượt, tổng tiền)
            int totalItems = await _context.DanhSachUngHos.CountAsync();
            ViewBag.TongSoLuot = totalItems;
            ViewBag.TongTien = await _context.DanhSachUngHos.SumAsync(x => (decimal?)x.SoTien) ?? 0;

            // --- MỚI THÊM: LẤY NGÀY ỦNG HỘ MỚI NHẤT ĐỂ HIỂN THỊ CHỮ "Số liệu cập nhật đến ngày..." ---
            var dongMoiNhat = await _context.DanhSachUngHos.OrderByDescending(x => x.NgayUngHo).FirstOrDefaultAsync();
            ViewBag.NgayCapNhat = dongMoiNhat != null ? dongMoiNhat.NgayUngHo.ToString("dd/MM/yyyy") : DateTime.Now.ToString("dd/MM/yyyy");
            // -----------------------------------------------------------------------------------------

            // 3. XỬ LÝ PHÂN TRANG (10 dòng / trang)
            int pageSize = 10;
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            // 4. LẤY DỮ LIỆU ĐỔ VÀO BẢNG (Đã cắt theo trang)
            ViewBag.DanhSachNguoiUngHo = await _context.DanhSachUngHos
                                                .OrderByDescending(x => x.NgayUngHo)
                                                .Skip((page - 1) * pageSize) // Bỏ qua các dòng của trang trước
                                                .Take(pageSize)              // Lấy đúng 10 dòng
                                                .ToListAsync();

            // Lấy dòng số dư mới nhất (sắp xếp giảm dần theo ngày, bốc ra dòng đầu tiên)
            ViewBag.SoDuQuy = await _context.SoDuQuyViNguoiNgheos.OrderByDescending(x => x.NgayCapNhat).FirstOrDefaultAsync();

            return View();
        }

        [Route("CongThongTinAnSXH/GetDanhSachUngHoPartial")]
        public async Task<IActionResult> GetDanhSachUngHoPartial(int page = 1)
        {
            int pageSize = 10;
            int totalItems = await _context.DanhSachUngHos.CountAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            ViewBag.DanhSachNguoiUngHo = await _context.DanhSachUngHos
                                                .OrderByDescending(x => x.NgayUngHo)
                                                .Skip((page - 1) * pageSize)
                                                .Take(pageSize)
                                                .ToListAsync();

            return PartialView("_DanhSachUngHoTable");
        }
    }
}