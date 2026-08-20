using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using WebMTTQ.Models;
using WebMTTQ.Services;

namespace WebMTTQ.Controllers
{
    public class QuyCuuTroController : Controller
    {
        private readonly DataMTTQContext _context;
        private readonly IMemoryCache _cache;
        private readonly ISystemSettingsService _settings;

        public QuyCuuTroController(DataMTTQContext context, IMemoryCache cache, ISystemSettingsService settings)
        {
            _context = context;
            _cache = cache;
            _settings = settings;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            // Kiểm tra bảo trì trang quỹ cứu trợ
            if (await MaintenanceHelper.IsQuyCuuTroUnderMaintenanceAsync(_settings))
            {
                return View("~/Views/Home/UnderConstruction.cshtml");
            }

            try
            {
                _context.Database.SetCommandTimeout(60);

                // 1. Tài khoản tiếp nhận
                ViewBag.DanhSachUngHo = await _context.ThongTinNhanUngHoCuuTros
                                                .Where(x => x.TrangThai == true)
                                                .AsNoTracking().ToListAsync();

                // 2. Thống kê
                if (!_cache.TryGetValue("ThongKeUngHoCuuTro", out ThongKeUngHoCuuTroDto? thongKe) || thongKe == null)
                {
                    thongKe = new ThongKeUngHoCuuTroDto
                    {
                        TotalItems = await _context.DanhSachUngHoCuuTros.Where(x => x.HienThi == true).CountAsync(),
                        TongTien = await _context.DanhSachUngHoCuuTros.Where(x => x.HienThi == true).SumAsync(x => (decimal?)x.SoTien) ?? 0,
                        NgayCapNhat = (await _context.DanhSachUngHoCuuTros
                                        .Where(x => x.HienThi == true)
                                        .OrderByDescending(x => x.NgayUngHo)
                                        .Select(x => x.NgayUngHo)
                                        .FirstOrDefaultAsync())?.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy")
                    };
                    _cache.Set("ThongKeUngHoCuuTro", thongKe, TimeSpan.FromMinutes(10));
                }

                ViewBag.TongSoLuot = thongKe!.TotalItems;
                ViewBag.TongTien = thongKe!.TongTien;
                ViewBag.NgayCapNhat = thongKe!.NgayCapNhat;

                // 4. Số dư quỹ
                ViewBag.SoDuQuy = await _context.SoDuQues
                                                .Where(x => x.LoaiQuy == "CuuTro")
                                                .AsNoTracking()
                                                .OrderByDescending(x => x.NgayCapNhat)
                                                .FirstOrDefaultAsync();

                // 5. Kết quả hoạt động
                int currentYear = DateTime.Now.Year;
                if (!_cache.TryGetValue("HoatDongCuuTroCache", out HoatDongCuuTroCacheDto? hoatDongData) || hoatDongData == null)
                {
                    var dsHoatDong = await _context.KetQuaHoatDongs
                                            .Where(x => x.LoaiHoatDong == "CuuTro" && x.TrangThai == true && x.Nam == currentYear)
                                            .AsNoTracking().ToListAsync();

                    hoatDongData = new HoatDongCuuTroCacheDto
                    {
                        TongKinhPhiCL = dsHoatDong.Sum(x => x.KinhPhi ?? 0),
                        TongLuotHoCL = dsHoatDong.Sum(x => x.SoLuongHo ?? 0),
                        TongHoatDongCL = dsHoatDong.Count,
                        TongDonViCL = dsHoatDong.Select(x => x.DonViUngHo).Distinct().Count(),
                        MaxThang = dsHoatDong.Any() ? dsHoatDong.Max(x => x.Thang ?? DateTime.Now.Month) : DateTime.Now.Month,
                        MinThang = dsHoatDong.Any() ? dsHoatDong.Min(x => x.Thang ?? DateTime.Now.Month) : DateTime.Now.Month,

                        ThongKeThang = dsHoatDong.GroupBy(x => x.Thang ?? 0)
                            .Select(g => new { Thang = g.Key, TongTien = g.Sum(x => x.KinhPhi ?? 0), SoHoatDong = g.Count() })
                            .OrderBy(x => x.Thang).Cast<dynamic>().ToList(),

                        ThongKeDonVi = dsHoatDong.GroupBy(x => string.IsNullOrEmpty(x.PhanLoaiDonVi) ? "Khác" : x.PhanLoaiDonVi)
                            .Select(g => new { TenLoai = g.Key, TongTien = g.Sum(x => x.KinhPhi ?? 0), SoHoatDong = g.Count() })
                            .OrderByDescending(x => x.TongTien).Cast<dynamic>().ToList(),

                        DanhSachChamLo = dsHoatDong.OrderByDescending(x => x.Thang).ThenByDescending(x => x.Id).ToList()
                    };

                    hoatDongData.MaxThangTien = hoatDongData.ThongKeThang.Any() ? hoatDongData.ThongKeThang.Max(x => (decimal)x.TongTien) : 1;
                    hoatDongData.ListNhomDonVi = hoatDongData.ThongKeDonVi.Select(x => (string)x.TenLoai).ToList();

                    _cache.Set("HoatDongCuuTroCache", hoatDongData, TimeSpan.FromMinutes(15));
                }

                ViewBag.TongKinhPhiCL = hoatDongData.TongKinhPhiCL;
                ViewBag.TongLuotHoCL = hoatDongData.TongLuotHoCL;
                ViewBag.TongHoatDongCL = hoatDongData.TongHoatDongCL;
                ViewBag.TongDonViCL = hoatDongData.TongDonViCL;
                ViewBag.ThangCapNhat = hoatDongData.MaxThang;
                ViewBag.ChuoiThang = $"Tháng {hoatDongData.MinThang} - Tháng {hoatDongData.MaxThang}/{currentYear}";
                ViewBag.ThongKeThang = hoatDongData.ThongKeThang;
                ViewBag.MaxThangTien = hoatDongData.MaxThangTien;
                ViewBag.ThongKeDonVi = hoatDongData.ThongKeDonVi;
                ViewBag.TongNhomDonVi = hoatDongData.ThongKeDonVi?.Count ?? 0;
                ViewBag.DanhSachChamLo = hoatDongData.DanhSachChamLo;
                ViewBag.ListNhomDonVi = hoatDongData.ListNhomDonVi;
                ViewBag.HienThiBanDo = false;
            }
            catch (Exception)
            {
                // Bẫy lỗi bảo vệ
                ViewBag.DanhSachUngHo = new List<ThongTinNhanUngHoCuuTro>();
                ViewBag.TongSoLuot = 0; ViewBag.TongTien = 0m; ViewBag.NgayCapNhat = DateTime.Now.ToString("dd/MM/yyyy");
                ViewBag.CurrentPage = 1; ViewBag.TotalPages = 1; ViewBag.DanhSachNguoiUngHo = new List<DanhSachUngHoCuuTro>();
                ViewBag.SoDuQuy = null; ViewBag.TongKinhPhiCL = 0m; ViewBag.TongLuotHoCL = 0; ViewBag.TongHoatDongCL = 0;
                ViewBag.TongDonViCL = 0; ViewBag.ThangCapNhat = DateTime.Now.Month; ViewBag.ChuoiThang = "Đang cập nhật";
                ViewBag.ThongKeThang = new List<dynamic>(); ViewBag.MaxThangTien = 1m; ViewBag.ThongKeDonVi = new List<dynamic>();
                ViewBag.TongNhomDonVi = 0; ViewBag.DanhSachChamLo = new List<dynamic>(); ViewBag.ListNhomDonVi = new List<string>();
                ViewBag.HienThiBanDo = false;
            }
            // ĐOẠN XỬ LÝ RIÊNG CHO PHÂN TRANG DANH SÁCH ỦNG HỘ:
            int pageSize = 10; // Tối đa 10 thông tin 1 trang
            var query = _context.DanhSachUngHoCuuTros.Where(x => x.HienThi == true);

            int totalItems = await query.CountAsync();
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Lấy 10 phần tử tương ứng với trang hiện tại
            ViewBag.DanhSachNguoiUngHo = await query
                .OrderByDescending(x => x.NgayUngHo)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return View("~/Views/CongThongTinAnSXH/QuyCuuTro.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuiYeuCauThamGia(string HoTen, string SoDienThoai, string DiaChi, string NoiDung, string MucDoUuTien)
        {
            if (!string.IsNullOrEmpty(HoTen) && !string.IsNullOrEmpty(SoDienThoai))
                TempData["SuccessMessage"] = "Gửi thông tin khẩn cấp thành công! UBMTTQ sẽ liên hệ để điều phối cứu trợ.";
            else
                TempData["ErrorMessage"] = "Vui lòng điền đầy đủ thông tin bắt buộc.";
            return RedirectToAction("Index", "QuyCuuTro", null, "nhu-cau-tro-giup");
        }
    }

    public class ThongKeUngHoCuuTroDto
    {
        public int TotalItems { get; set; }
        public decimal TongTien { get; set; }
        public string? NgayCapNhat { get; set; }
    }

    public class HoatDongCuuTroCacheDto
    {
        public decimal TongKinhPhiCL { get; set; }
        public int TongLuotHoCL { get; set; }
        public int TongHoatDongCL { get; set; }
        public int TongDonViCL { get; set; }
        public int MaxThang { get; set; }
        public int MinThang { get; set; }
        public decimal MaxThangTien { get; set; }
        public List<dynamic>? ThongKeThang { get; set; }
        public List<dynamic>? ThongKeDonVi { get; set; }
        public List<string>? ListNhomDonVi { get; set; }
        public object? DanhSachChamLo { get; set; }
    }
}