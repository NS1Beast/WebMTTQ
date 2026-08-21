using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic; // Bổ sung thư viện này để dùng List
using WebMTTQ.Models;
using WebMTTQ.Services;

namespace WebMTTQ.Controllers
{
    public class CongThongTinAnSXHController : Controller
    {
        private readonly DataMTTQContext _context;
        private readonly IMemoryCache _cache; // Thêm bộ nhớ đệm
        private readonly ISystemSettingsService _settings;

        // Tiêm IMemoryCache vào constructor
        public CongThongTinAnSXHController(DataMTTQContext context, IMemoryCache cache, ISystemSettingsService settings)
        {
            _context = context;
            _cache = cache;
            _settings = settings;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            // Kiểm tra bảo trì trang an sinh xã hội
            if (await MaintenanceHelper.IsAnSinhXaHoiUnderMaintenanceAsync(_settings))
            {
                return View("~/Views/Home/UnderConstruction.cshtml");
            }

            try
            {
                // BƯỚC KHẮC PHỤC 1: Báo cho CSDL ráng đợi 60 giây, không được văng lỗi vội
                _context.Database.SetCommandTimeout(60);

                // 1. TỐI ƯU THÔNG TIN NHẬN ỦNG HỘ (Dùng AsNoTracking vì chỉ hiển thị)
                ViewBag.DanhSachUngHo = await _context.ThongTinNhanUngHos.AsNoTracking().ToListAsync();

                // 2. TỐI ƯU THỐNG KÊ (Sử dụng Cache 10 phút)
                // Sửa 1: Đổi thành ThongKeUngHoDto? (thêm dấu ?) và đồng nhất tên biến là thongKe (chữ K hoa)
                if (!_cache.TryGetValue("ThongKeUngHo", out ThongKeUngHoDto? thongKe) || thongKe == null)
                {
                    thongKe = new ThongKeUngHoDto
                    {
                        TotalItems = await _context.DanhSachUngHos.CountAsync(),
                        TongTien = await _context.DanhSachUngHos.SumAsync(x => (decimal?)x.SoTien) ?? 0,
                        NgayCapNhat = await _context.DanhSachUngHos.OrderByDescending(x => x.NgayUngHo)
                                        .Select(x => x.NgayUngHo.ToString("dd/MM/yyyy"))
                                        .FirstOrDefaultAsync() ?? DateTime.Now.ToString("dd/MM/yyyy")
                    };
                    _cache.Set("ThongKeUngHo", thongKe, TimeSpan.FromMinutes(10));
                }

                // Sửa 2: Thêm dấu chấm than (!) sau thongKe để báo với trình biên dịch rằng: "Biến này chắc chắn không bị null đâu, yên tâm!"
                ViewBag.TongSoLuot = thongKe!.TotalItems;
                ViewBag.TongTien = thongKe!.TongTien;
                ViewBag.NgayCapNhat = thongKe!.NgayCapNhat;

                // 3. TỐI ƯU PHÂN TRANG (Chỉ lấy đúng 10 dòng của trang đó, kết hợp AsNoTracking)
                int pageSize = 10;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling(thongKe.TotalItems / (double)pageSize);

                ViewBag.DanhSachNguoiUngHo = await _context.DanhSachUngHos
                                                .AsNoTracking()
                                                .OrderByDescending(x => x.NgayUngHo)
                                                .Skip((page - 1) * pageSize)
                                                .Take(pageSize)
                                                .ToListAsync();

                // 4. LẤY SỐ DƯ QUỸ (Chỉ lấy 1 dòng mới nhất, không Tracking)
                ViewBag.SoDuQuy = await _context.SoDuQues
                                                .Where(x => x.LoaiQuy == "NguoiNgheo")
                                                .AsNoTracking()
                                                .OrderByDescending(x => x.NgayCapNhat)
                                                .FirstOrDefaultAsync();

                // 5. TỐI ƯU KẾT QUẢ CHĂM LO (Dùng Cache vì dữ liệu này tính toán rất nặng)
                if (!_cache.TryGetValue("KetQuaChamLoData", out ChamLoCacheDto? chamLoData) || chamLoData == null)
                {
                    var dsChamLo = await _context.KetQuaChamLos.AsNoTracking().ToListAsync();

                    chamLoData = new ChamLoCacheDto
                    {
                        TongKinhPhiCL = dsChamLo.Sum(x => x.KinhPhi),
                        TongLuotHoCL = dsChamLo.Sum(x => x.SoLuongHo),
                        TongHoatDongCL = dsChamLo.Count,
                        TongDonViCL = dsChamLo.Select(x => x.DonViUngHo).Distinct().Count(),
                        MaxThang = dsChamLo.Any() ? dsChamLo.Max(x => x.Thang) : DateTime.Now.Month,
                        MinThang = dsChamLo.Any() ? dsChamLo.Min(x => x.Thang) : DateTime.Now.Month,

                        ThongKeThang = dsChamLo.GroupBy(x => x.Thang)
                            .Select(g => new { Thang = g.Key, TongTien = g.Sum(x => x.KinhPhi), SoHoatDong = g.Count() })
                            .OrderBy(x => x.Thang).Cast<dynamic>().ToList(),

                        ThongKeDonVi = dsChamLo.GroupBy(x => x.PhanLoaiDonVi)
                            .Select(g => new { TenLoai = g.Key, TongTien = g.Sum(x => x.KinhPhi), SoHoatDong = g.Count() })
                            .OrderByDescending(x => x.TongTien).Cast<dynamic>().ToList(),

                        DanhSachChamLo = dsChamLo.OrderByDescending(x => x.Thang).ThenByDescending(x => x.Id).ToList()
                    };

                    chamLoData.MaxThangTien = chamLoData.ThongKeThang.Any() ? chamLoData.ThongKeThang.Max(x => (decimal)x.TongTien) : 1;
                    chamLoData.ListNhomDonVi = chamLoData.ThongKeDonVi.Select(x => (string)x.TenLoai).ToList();

                    _cache.Set("KetQuaChamLoData", chamLoData, TimeSpan.FromMinutes(15));
                }

                ViewBag.TongKinhPhiCL = chamLoData.TongKinhPhiCL;
                ViewBag.TongLuotHoCL = chamLoData.TongLuotHoCL;
                ViewBag.TongHoatDongCL = chamLoData.TongHoatDongCL;
                ViewBag.TongDonViCL = chamLoData.TongDonViCL;
                ViewBag.ThangCapNhat = chamLoData.MaxThang;
                ViewBag.ChuoiThang = $"Tháng {chamLoData.MinThang} - Tháng {chamLoData.MaxThang}/{DateTime.Now.Year}";
                ViewBag.ThongKeThang = chamLoData.ThongKeThang;
                ViewBag.MaxThangTien = chamLoData.MaxThangTien;
                ViewBag.ThongKeDonVi = chamLoData.ThongKeDonVi;
                ViewBag.TongNhomDonVi = chamLoData.ThongKeDonVi?.Count ?? 0;
                ViewBag.DanhSachChamLo = chamLoData.DanhSachChamLo;
                ViewBag.ListNhomDonVi = chamLoData.ListNhomDonVi;

                // ===== TẠM ẨN BẢN ĐỒ: Để phát triển lại sau này =====
                // Đặt flag false để ẩn bản đồ, khi cần hiện lại thì đổi thành true
                ViewBag.HienThiBanDo = false;

                //if (ViewBag.HienThiBanDo == true)
                //{
                //    // 6. TỐI ƯU BẢN ĐỒ (Dùng Cache cho JSON bản đồ để tải web nhanh hơn)
                //    if (!_cache.TryGetValue("BanDoData", out BanDoCacheDto? banDoData) || banDoData == null)
                //    {
                //        var rawData = await _context.DiaDiemBanDos
                //                                    .AsNoTracking()
                //                                    .OrderByDescending(x => x.NgayThucHien)
                //                                    .ToListAsync();

                //        var mapList = rawData.Select(x => new {
                //            id = x.IddiaDiem,
                //            ten = x.TenDiaDiem,
                //            phanLoai = x.PhanLoaiBanDo,
                //            viDo = x.ViDo,
                //            kinhDo = x.KinhDo,
                //            moTa = x.MoTaChiTiet ?? "",
                //            ngay = x.NgayThucHien.HasValue ? x.NgayThucHien.Value.ToString("dd/MM/yyyy") : "Đang cập nhật",
                //            diaChi = x.DiaChi ?? "",
                //            hinhAnh = x.HinhAnhThucTe != null ? "data:image/jpeg;base64," + Convert.ToBase64String(x.HinhAnhThucTe) : ""
                //        }).ToList();

                //        banDoData = new BanDoCacheDto
                //        {
                //            TongDiaDiem = rawData.Count,
                //            NhomDonVi = rawData.Select(x => x.PhanLoaiBanDo).Distinct().Count(),
                //            DanhSachNhom = rawData.Select(x => x.PhanLoaiBanDo).Distinct().ToList(),
                //            MapDataJson = System.Text.Json.JsonSerializer.Serialize(mapList)
                //        };
                //        _cache.Set("BanDoData", banDoData, TimeSpan.FromMinutes(30)); // Đợi 30p mới query DB lại
                //    }

                //    ViewBag.TongDiaDiem = banDoData.TongDiaDiem;
                //    ViewBag.NhomDonVi = banDoData.NhomDonVi;
                //    ViewBag.DanhSachNhom = banDoData.DanhSachNhom;
                //    ViewBag.MapDataJson = banDoData.MapDataJson;
                //}
                //else
                //{
                //    // Gán giá trị rỗng khi ẩn bản đồ
                //    ViewBag.TongDiaDiem = 0;
                //    ViewBag.NhomDonVi = 0;
                //    ViewBag.DanhSachNhom = new List<string>();
                //    ViewBag.MapDataJson = "[]";
                //}
            }
            catch (Exception ex)
            {
                // BƯỚC KHẮC PHỤC 2: Nếu có lỗi (Timeout), chạy vào đây gán giá trị rỗng để bảo vệ giao diện không bị sập trắng
                Console.WriteLine("LỖI KẾT NỐI DB TẠI CỔNG AN SINH: " + ex.Message);

                ViewBag.DanhSachUngHo = new List<ThongTinNhanUngHo>();
                ViewBag.TongSoLuot = 0;
                ViewBag.TongTien = 0m;
                ViewBag.NgayCapNhat = DateTime.Now.ToString("dd/MM/yyyy");
                ViewBag.CurrentPage = 1;
                ViewBag.TotalPages = 1;
                ViewBag.DanhSachNguoiUngHo = new List<DanhSachUngHo>();
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

            return View();
        }

        [Route("CongThongTinAnSXH/GetDanhSachUngHoPartial")]
        public async Task<IActionResult> GetDanhSachUngHoPartial(int page = 1)
        {
            try
            {
                _context.Database.SetCommandTimeout(60);
                int pageSize = 10;
                // Dùng Cache cho đếm tổng số dòng
                int totalItems = await _cache.GetOrCreateAsync("TotalItemsUngHo", async entry => {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                    return await _context.DanhSachUngHos.CountAsync();
                });

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                ViewBag.DanhSachNguoiUngHo = await _context.DanhSachUngHos
                                                .AsNoTracking() // Tối ưu
                                                .OrderByDescending(x => x.NgayUngHo)
                                                .Skip((page - 1) * pageSize)
                                                .Take(pageSize)
                                                .ToListAsync();
            }
            catch (Exception)
            {
                ViewBag.CurrentPage = 1;
                ViewBag.TotalPages = 1;
                ViewBag.DanhSachNguoiUngHo = new List<DanhSachUngHo>();
            }

            return PartialView("_DanhSachUngHoTable");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuiYeuCauTroGiup(NguoiDanCanTroGiup model)
        {
            // Kiểm tra bảo trì trang an sinh xã hội
            if (await MaintenanceHelper.IsAnSinhXaHoiUnderMaintenanceAsync(_settings))
            {
                return View("~/Views/Home/UnderConstruction.cshtml");
            }

            // === Chống spam: kiểm tra thời gian giữa các lần gửi (tối thiểu 30 giây) ===
            var lastSubmitKey = $"TroGiup_LastSubmit_{HttpContext.Connection.RemoteIpAddress}";
            var lastSubmit = HttpContext.Session.GetString(lastSubmitKey);
            if (!string.IsNullOrEmpty(lastSubmit) && long.TryParse(lastSubmit, out var lastTicks))
            {
                var lastTime = new DateTime(lastTicks);
                if ((DateTime.Now - lastTime).TotalSeconds < 30)
                {
                    TempData["ErrorMessage"] = "Bạn vừa gửi yêu cầu trợ giúp. Vui lòng chờ 30 giây trước khi gửi lại.";
                    return RedirectToAction("Index", null, "nhu-cau-tro-giup");
                }
            }

            // === Sanitize dữ liệu đầu vào ===
            if (model.HoTen != null) model.HoTen = model.HoTen.Trim();
            if (model.SoDienThoai != null) model.SoDienThoai = model.SoDienThoai.Trim();
            if (model.DiaChi != null) model.DiaChi = model.DiaChi.Trim();
            if (model.NoiDung != null) model.NoiDung = model.NoiDung.Trim();
            if (model.MucDoUuTien != null) model.MucDoUuTien = model.MucDoUuTien.Trim();

            if (ModelState.IsValid)
            {
                model.NgayGui = DateTime.Now;
                model.TrangThai = "Chưa xử lý";

                _context.NguoiDanCanTroGiups.Add(model);
                await _context.SaveChangesAsync();

                // Lưu thời gian gửi cuối cùng để chống spam
                HttpContext.Session.SetString(lastSubmitKey, DateTime.Now.Ticks.ToString());

                // Trả về thông báo thành công
                TempData["SuccessMessage"] = "Gửi thông tin thành công! UBMTTQ sẽ liên hệ với bạn trong thời gian sớm nhất.";
                return RedirectToAction("Index", null, "nhu-cau-tro-giup"); // Trở lại trang chủ và cuộn đúng vị trí form
            }

            TempData["ErrorMessage"] = "Vui lòng điền đầy đủ thông tin bắt buộc và đúng định dạng.";
            return RedirectToAction("Index", null, "nhu-cau-tro-giup");
        }
    }

    // Các Class DTO hỗ trợ cho việc Cache phía trên
    public class ThongKeUngHoDto
    {
        public int TotalItems { get; set; }
        public decimal TongTien { get; set; }
        public string ? NgayCapNhat { get; set; }
    }
    public class ChamLoCacheDto
    {
        public decimal TongKinhPhiCL { get; set; }
        public int TongLuotHoCL { get; set; }
        public int TongHoatDongCL { get; set; }
        public int TongDonViCL { get; set; }
        public int MaxThang { get; set; }
        public int MinThang { get; set; }
        public decimal MaxThangTien { get; set; }
        public List<dynamic> ? ThongKeThang { get; set; }
        public List<dynamic> ? ThongKeDonVi { get; set; }
        public List<string> ? ListNhomDonVi { get; set; }
        public object ? DanhSachChamLo { get; set; }
    }
    public class BanDoCacheDto
    {
        public int TongDiaDiem { get; set; }
        public int NhomDonVi { get; set; }
        public List<string> ? DanhSachNhom { get; set; }
        public string ? MapDataJson { get; set; }
    }
}