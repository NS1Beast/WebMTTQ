using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using WebMTTQ.Models; // Đảm bảo namespace này khớp với project của bạn

namespace WebMTTQ.Controllers
{
    public class QuyBienDaoController : Controller
    {
        private readonly DataMTTQContext _context;
        private readonly IMemoryCache _cache;

        public QuyBienDaoController(DataMTTQContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // Action này phải trùng tên với thẻ asp-action trong _Layout.cshtml của bạn
        public async Task<IActionResult> ViBienDaoQueHuongViTuyenDauTQ(int page = 1)
        {
            try
            {
                // BƯỚC KHẮC PHỤC 1: Báo cho CSDL ráng đợi 60 giây, không được văng lỗi vội
                _context.Database.SetCommandTimeout(60);

                // 1. TỐI ƯU THÔNG TIN NHẬN ỦNG HỘ (Dùng AsNoTracking vì chỉ hiển thị)
                ViewBag.DanhSachUngHo = await _context.ThongTinNhanUngHoBienDaos
                                                .Where(x => x.TrangThai == true)
                                                .AsNoTracking()
                                                .ToListAsync();

                // 2. TỐI ƯU THỐNG KÊ (Sử dụng Cache 10 phút)
                if (!_cache.TryGetValue("ThongKeUngHoBienDao", out ThongKeUngHoBienDaoDto? thongKe) || thongKe == null)
                {
                    thongKe = new ThongKeUngHoBienDaoDto
                    {
                        TotalItems = await _context.DanhSachUngHoBienDaos.Where(x => x.HienThi == true).CountAsync(),
                        TongTien = await _context.DanhSachUngHoBienDaos.Where(x => x.HienThi == true).SumAsync(x => (decimal?)x.SoTien) ?? 0,
                        // Xóa đoạn cũ và thay bằng đoạn này:
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

                //// 3. TỐI ƯU PHÂN TRANG (Chỉ lấy đúng 10 dòng của trang đó, kết hợp AsNoTracking)
                //int pageSize = 10;
                //ViewBag.CurrentPage = page;
                //ViewBag.TotalPages = (int)Math.Ceiling(thongKe.TotalItems / (double)pageSize);

                //ViewBag.DanhSachNguoiUngHo = await _context.DanhSachUngHoBienDaos
                //                                .Where(x => x.HienThi == true)
                //                                .AsNoTracking()
                //                                .OrderByDescending(x => x.NgayUngHo)
                //                                .Skip((page - 1) * pageSize)
                //                                .Take(pageSize)
                //                                .ToListAsync();

                // 4. LẤY SỐ DƯ QUỸ (Chỉ lấy 1 dòng mới nhất, không Tracking)
                ViewBag.SoDuQuy = await _context.SoDuQuyBienDaos
                                                .AsNoTracking()
                                                .OrderByDescending(x => x.NgayCapNhat)
                                                .FirstOrDefaultAsync();

                // 5. TỐI ƯU KẾT QUẢ HOẠT ĐỘNG BIỂN ĐẢO (Dùng Cache vì dữ liệu này tính toán rất nặng)
                int currentYear = DateTime.Now.Year;
                if (!_cache.TryGetValue("HoatDongBienDaoCache", out HoatDongBienDaoCacheDto? hoatDongData) || hoatDongData == null)
                {
                    var dsHoatDong = await _context.KetQuaHoatDongBienDaos
                                            .Where(x => x.TrangThai == true && x.Nam == currentYear)
                                            .AsNoTracking()
                                            .ToListAsync();
                    hoatDongData = new HoatDongBienDaoCacheDto
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
                throw ex;
                // BƯỚC KHẮC PHỤC 2: Nếu có lỗi (Timeout), chạy vào đây gán giá trị rỗng để bảo vệ giao diện không bị sập trắng
                Console.WriteLine("LỖI KẾT NỐI DB TẠI CỔNG BIỂN ĐẢO: " + ex.Message);

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
                ViewBag.ThongKeThang = new List<dynamic>();
                ViewBag.MaxThangTien = 1m;
                ViewBag.ThongKeDonVi = new List<dynamic>();
                ViewBag.TongNhomDonVi = 0;
                ViewBag.DanhSachChamLo = new List<dynamic>();
                ViewBag.ListNhomDonVi = new List<string>();

                ViewBag.TongDiaDiem = 0;
                ViewBag.NhomDonVi = 0;
                ViewBag.DanhSachNhom = new List<string>();
                ViewBag.MapDataJson = "[]";
                ViewBag.HienThiBanDo = false;
            }
            int pageSize = 10; // Tối đa 10 thông tin 1 trang
            var query = _context.DanhSachUngHoBienDaos.Where(x => x.HienThi == true);

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

            // Mặc định nó sẽ tìm file Views/QuyBienDao/ViBienDaoQueHuongViTuyenDauTQ.cshtml
            // Nếu View của bạn đang nằm ở Views/CongThongTinAnSXH/, bạn cần chỉ định rõ đường dẫn:
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

            // Nếu bạn có file PartialView riêng cho Biển Đảo, hãy điền đường dẫn vào đây
            return PartialView("~/Views/CongThongTinAnSXH/_DanhSachUngHoTable.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuiYeuCauThamGia(string HoTen, string SoDienThoai, string DiaChi, string NoiDung, string MucDoUuTien)
        {
            // Do bạn chưa có bảng CSDL để lưu form Biển Đảo, tạm thời xử lý Logic giả lập.
            // Nếu sau này có bảng, bạn ánh xạ model tương tự hàm GuiYeuCauTroGiup ở trang bên kia.

            if (!string.IsNullOrEmpty(HoTen) && !string.IsNullOrEmpty(SoDienThoai))
            {
                TempData["SuccessMessage"] = "Gửi thông tin thành công! UBMTTQ sẽ liên hệ với bạn trong thời gian sớm nhất để cùng chung tay hướng về Biển Đảo.";
            }
            else
            {
                TempData["ErrorMessage"] = "Vui lòng điền đầy đủ thông tin bắt buộc.";
            }

            // Redirect về đúng Action xử lý giao diện
            return RedirectToAction("ViBienDaoQueHuongViTuyenDauTQ", "QuyBienDao", null, "nhu-cau-tro-giup");
        }
    }

    // Các Class DTO hỗ trợ cho việc Cache phía trên (Sửa tên để không bị trùng lặp với file cũ)
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