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
    public class QuyBienDaoController : Controller
    {
        private readonly DataMTTQContext _context;
        private readonly IMemoryCache _cache;
        private readonly ISystemSettingsService _settings;

        public QuyBienDaoController(DataMTTQContext context, IMemoryCache cache, ISystemSettingsService settings)
        {
            _context = context;
            _cache = cache;
            _settings = settings;
        }

        // Action này phải trùng tên với thẻ asp-action trong _Layout.cshtml của bạn
        public async Task<IActionResult> ViBienDaoQueHuongViTuyenDauTQ(int page = 1)
        {
            // Kiểm tra bảo trì trang quỹ biển đảo
            if (await MaintenanceHelper.IsQuyBienDaoUnderMaintenanceAsync(_settings))
            {
                return View("~/Views/Home/UnderConstruction.cshtml");
            }

            try
            {
                _context.Database.SetCommandTimeout(60);

                // 1. TỐI ƯU THÔNG TIN NHẬN ỦNG HỘ
                ViewBag.DanhSachUngHo = await _context.ThongTinNhanUngHoBienDaos
                                                .Where(x => x.TrangThai == true)
                                                .AsNoTracking()
                                                .ToListAsync();

                // 2. TỐI ƯU THỐNG KÊ
                if (!_cache.TryGetValue("ThongKeUngHoBienDao", out ThongKeUngHoBienDaoDto? thongKe) || thongKe == null)
                {
                    thongKe = new ThongKeUngHoBienDaoDto
                    {
                        TotalItems = await _context.DanhSachUngHoBienDaos.Where(x => x.HienThi == true).CountAsync(),
                        TongTien = await _context.DanhSachUngHoBienDaos.Where(x => x.HienThi == true).SumAsync(x => (decimal?)x.SoTien) ?? 0,
                        NgayCapNhat = (await _context.DanhSachUngHoBienDaos
                                        .Where(x => x.HienThi == true)
                                        .OrderByDescending(x => x.NgayUngHo)
                                        .Select(x => x.NgayUngHo)
                                        .FirstOrDefaultAsync())?.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy")
                    };
                    _cache.Set("ThongKeUngHoBienDao", thongKe, TimeSpan.FromMinutes(10));
                }

                ViewBag.TongSoLuot = thongKe!.TotalItems;
                ViewBag.TongTien = thongKe!.TongTien;
                ViewBag.NgayCapNhat = thongKe!.NgayCapNhat;

                // 4. LẤY SỐ DƯ QUỸ
                ViewBag.SoDuQuy = await _context.SoDuQues
                                                .Where(x => x.LoaiQuy == "BienDao")
                                                .AsNoTracking()
                                                .OrderByDescending(x => x.NgayCapNhat)
                                                .FirstOrDefaultAsync();

                // 5. TỐI ƯU KẾT QUẢ HOẠT ĐỘNG BIỂN ĐẢO
                int currentYear = DateTime.Now.Year;
                if (!_cache.TryGetValue("HoatDongBienDaoCache", out HoatDongBienDaoCacheDto? hoatDongData) || hoatDongData == null)
                {
                    var dsHoatDong = await _context.KetQuaHoatDongs
                                            .Where(x => x.LoaiHoatDong == "BienDao" && x.TrangThai == true && x.Nam == currentYear)
                                            .AsNoTracking()
                                            .ToListAsync();
                    hoatDongData = new HoatDongBienDaoCacheDto
                    {
                        TongKinhPhiCL = dsHoatDong.Sum(d => d.KinhPhi ?? 0),
                        TongLuotHoCL = dsHoatDong.Sum(d => d.SoLuongHo ?? 0),
                        TongHoatDongCL = dsHoatDong.Count,
                        TongDonViCL = dsHoatDong.Select(d => d.DonViUngHo).Distinct().Count(),
                        MaxThang = dsHoatDong.Any() ? dsHoatDong.Max(d => d.Thang ?? DateTime.Now.Month) : DateTime.Now.Month,
                        MinThang = dsHoatDong.Any() ? dsHoatDong.Min(d => d.Thang ?? DateTime.Now.Month) : DateTime.Now.Month,

                        ThongKeThang = dsHoatDong.GroupBy(d => d.Thang ?? 0)
                            .Select(g => new { Thang = g.Key, TongTien = g.Sum(d => d.KinhPhi ?? 0), SoHoatDong = g.Count() })
                            .OrderBy(d => d.Thang).Cast<dynamic>().ToList(),

                        ThongKeDonVi = dsHoatDong.GroupBy(d => string.IsNullOrEmpty(d.PhanLoaiDonVi) ? "Khác" : d.PhanLoaiDonVi)
                            .Select(g => new { TenLoai = g.Key, TongTien = g.Sum(d => d.KinhPhi ?? 0), SoHoatDong = g.Count() })
                            .OrderByDescending(d => d.TongTien).Cast<dynamic>().ToList(),

                        DanhSachChamLo = dsHoatDong.OrderByDescending(d => d.Thang).ThenByDescending(d => d.Id).ToList()
                    };
                    hoatDongData.MaxThangTien = hoatDongData.ThongKeThang.Any() ? hoatDongData.ThongKeThang.Max(d => (decimal)d.TongTien) : 1;
                    hoatDongData.ListNhomDonVi = hoatDongData.ThongKeDonVi.Select(d => (string)d.TenLoai).ToList();

                    _cache.Set("HoatDongBienDaoCache", hoatDongData, TimeSpan.FromMinutes(15));
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

                // ===== TẠM ẨN BẢN ĐỒ BIỂN ĐẢO =====
                ViewBag.HienThiBanDo = false;
                ViewBag.TongDiaDiem = 0;
                ViewBag.NhomDonVi = 0;
                ViewBag.DanhSachNhom = new List<string>();
                ViewBag.MapDataJson = "[]";
            }
            catch (Exception ex)
            {
                Console.WriteLine("LOI KET NOI DB TAI BIEN DAO: " + ex.Message);

                ViewBag.DanhSachUngHo = new List<ThongTinNhanUngHoBienDao>();
                ViewBag.TongSoLuot = 0;
                ViewBag.TongTien = 0m;
                ViewBag.NgayCapNhat = DateTime.Now.ToString("dd/MM/yyyy");
                ViewBag.CurrentPage = 1;
                ViewBag.TotalPages = 1;
                ViewBag.DanhSachNguoiUngHo = new List<DanhSachUngHoBienDao>();
                ViewBag.SoDuQuy = null;

                ViewBag.TongKinhPhiCL = 0m;
                ViewBag.TongLuotHoCL = 0;
                ViewBag.TongHoatDongCL = 0;
                ViewBag.TongDonViCL = 0;
                ViewBag.ThangCapNhat = DateTime.Now.Month;
                ViewBag.ChuoiThang = "Đang cập nhật";
                ViewBag.ThongKeThang = new List<object>();
                ViewBag.ThongKeDonVi = new List<object>();
                ViewBag.TongNhomDonVi = 0;
                ViewBag.DanhSachChamLo = new List<object>();
                ViewBag.ListNhomDonVi = new List<string>();

                ViewBag.TongDiaDiem = 0;
                ViewBag.NhomDonVi = 0;
                ViewBag.DanhSachNhom = new List<string>();
                ViewBag.MapDataJson = "[]";
                ViewBag.HienThiBanDo = false;
            }
            int pageSize = 10;
            var query = _context.DanhSachUngHoBienDaos.Where(x => x.HienThi == true);

            int totalItems = await query.CountAsync();
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            ViewBag.DanhSachNguoiUngHo = await query
                .OrderByDescending(x => x.NgayUngHo)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return View("~/Views/CongThongTinAnSXH/ViBienDaoQueHuongViTuyenDauTQ.cshtml");
        }

        [Route("QuyBienDao/GetDanhSachUngHoPartial")]
        public async Task<IActionResult> GetDanhSachUngHoPartial(int page = 1)
        {
            try
            {
                _context.Database.SetCommandTimeout(60);
                int pageSize = 10;

                int totalItems = await _cache.GetOrCreateAsync("TotalItemsUngHoBienDao", async entry => {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                    return await _context.DanhSachUngHoBienDaos.Where(x => x.HienThi == true).CountAsync();
                });

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                ViewBag.DanhSachNguoiUngHo = await _context.DanhSachUngHoBienDaos
                                                .Where(x => x.HienThi == true)
                                                .AsNoTracking()
                                                .OrderByDescending(x => x.NgayUngHo)
                                                .Skip((page - 1) * pageSize)
                                                .Take(pageSize)
                                                .ToListAsync();
            }
            catch (Exception)
            {
                ViewBag.CurrentPage = 1;
                ViewBag.TotalPages = 1;
                ViewBag.DanhSachNguoiUngHo = new List<DanhSachUngHoBienDao>();
            }

            return PartialView("~/Views/CongThongTinAnSXH/_DanhSachUngHoTable.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuiYeuCauThamGia(string HoTen, string SoDienThoai, string DiaChi, string NoiDung, string MucDoUuTien)
        {
            if (!string.IsNullOrEmpty(HoTen) && !string.IsNullOrEmpty(SoDienThoai))
            {
                TempData["SuccessMessage"] = "Gửi thông tin thành công! UBMTTQ sẽ liên hệ với bạn trong thời gian sớm nhất để cùng chung tay hướng về Biển Đảo.";
            }
            else
            {
                TempData["ErrorMessage"] = "Vui lòng điền đầy đủ thông tin bắt buộc.";
            }

            return RedirectToAction("ViBienDaoQueHuongViTuyenDauTQ", "QuyBienDao", null, "nhu-cau-tro-giup");
        }
    }

    public class ThongKeUngHoBienDaoDto
    {
        public int TotalItems { get; set; }
        public decimal TongTien { get; set; }
        public string? NgayCapNhat { get; set; }
    }

    public class HoatDongBienDaoCacheDto
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