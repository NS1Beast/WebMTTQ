using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System;
using OfficeOpenXml;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebMTTQ.Models;

namespace WebMTTQ.Controllers
{
    // CHIÊU THỨC TRỊ LỖI 404 TẬN GỐC: 
    // Chấp nhận cả 2 loại đường dẫn (có dấu gạch chéo và không có dấu gạch chéo)!
    [Route("AdminQuyCuuTro/[action]/{id?}")]
    [Route("Admin/QuyCuuTro/[action]/{id?}")]
    public class AdminQuyCuuTroController : Controller
    {
        private readonly DataMTTQContext _context;
        private readonly IWebHostEnvironment _env;

        public AdminQuyCuuTroController(DataMTTQContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ==========================================
        // 1. THÔNG TIN TIẾP NHẬN
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> ThongTin()
        {
            ViewData["Title"] = "Tài khoản tiếp nhận Cứu trợ";
            var data = await _context.ThongTinNhanUngHoCuuTros.OrderByDescending(x => x.Id).ToListAsync();
            return View("~/Views/Admin/QuyCuuTro/ThongTin.cshtml", data);
        }

        [HttpGet]
        public IActionResult ThemThongTin() => View("~/Views/Admin/QuyCuuTro/ThemThongTin.cshtml");

        [HttpPost]
        public async Task<IActionResult> ThemThongTin(ThongTinNhanUngHoCuuTro model, IFormFile? QrCodeFile)
        {
            if (QrCodeFile != null && QrCodeFile.Length > 0)
            {
                if (QrCodeFile.Length > 2 * 1024 * 1024)
                {
                    TempData["ErrorMessage"] = "Kích thước ảnh vượt quá 2MB!";
                    return View("~/Views/Admin/QuyCuuTro/ThemThongTin.cshtml", model);
                }
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "qrcodes");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(QrCodeFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await QrCodeFile.CopyToAsync(fileStream);
                }
                model.QrCodeUrl = "/uploads/qrcodes/" + uniqueFileName;
            }
            model.TrangThai = true;
            _context.ThongTinNhanUngHoCuuTros.Add(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Thêm tài khoản thành công!";
            return RedirectToAction("ThongTin");
        }

        [HttpGet]
        public async Task<IActionResult> SuaThongTin(int id)
        {
            var item = await _context.ThongTinNhanUngHoCuuTros.FindAsync(id);
            if (item == null) return NotFound();
            return View("~/Views/Admin/QuyCuuTro/SuaThongTin.cshtml", item);
        }

        [HttpPost]
        public async Task<IActionResult> SuaThongTin(ThongTinNhanUngHoCuuTro model, IFormFile? QrCodeFile)
        {
            if (QrCodeFile != null && QrCodeFile.Length > 0)
            {
                if (QrCodeFile.Length > 2 * 1024 * 1024)
                {
                    TempData["ErrorMessage"] = "Kích thước ảnh vượt quá 2MB!";
                    return RedirectToAction("SuaThongTin", new { id = model.Id });
                }
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "qrcodes");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(QrCodeFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await QrCodeFile.CopyToAsync(fileStream);
                }
                model.QrCodeUrl = "/uploads/qrcodes/" + uniqueFileName;
            }
            _context.ThongTinNhanUngHoCuuTros.Update(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật tài khoản thành công!";
            return RedirectToAction("ThongTin");
        }

        [HttpPost]
        public async Task<IActionResult> XoaThongTin(int id)
        {
            var item = await _context.ThongTinNhanUngHoCuuTros.FindAsync(id);
            if (item != null)
            {
                _context.ThongTinNhanUngHoCuuTros.Remove(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa tài khoản!";
            }
            return RedirectToAction("ThongTin");
        }

        // ==========================================
        // 2. SỐ DƯ QUỸ
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> SoDu()
        {
            ViewData["Title"] = "Số dư Quỹ Cứu trợ";
            var data = await _context.SoDuQuyCuuTros.OrderByDescending(x => x.NgayCapNhat).ToListAsync();
            return View("~/Views/Admin/QuyCuuTro/SoDu.cshtml", data);
        }

        [HttpGet]
        public IActionResult ThemSoDu() => View("~/Views/Admin/QuyCuuTro/ThemSoDu.cshtml");

        [HttpPost]
        public async Task<IActionResult> ThemSoDu(SoDuQuyCuuTro model)
        {
            model.NgayCapNhat = DateTime.Now;
            model.TongTonQuy = (model.TienMat ?? 0) + (model.TienGuiNganHang ?? 0);
            _context.SoDuQuyCuuTros.Add(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật số dư mới thành công!";
            return RedirectToAction("SoDu");
        }

        [HttpGet]
        public async Task<IActionResult> SuaSoDu(int id)
        {
            var item = await _context.SoDuQuyCuuTros.FindAsync(id);
            if (item == null) return NotFound();
            return View("~/Views/Admin/QuyCuuTro/SuaSoDu.cshtml", item);
        }

        [HttpPost]
        public async Task<IActionResult> SuaSoDu(SoDuQuyCuuTro model)
        {
            model.TongTonQuy = (model.TienMat ?? 0) + (model.TienGuiNganHang ?? 0);
            _context.SoDuQuyCuuTros.Update(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật số dư thành công!";
            return RedirectToAction("SoDu");
        }

        [HttpPost]
        public async Task<IActionResult> XoaSoDu(int id)
        {
            var item = await _context.SoDuQuyCuuTros.FindAsync(id);
            if (item != null)
            {
                _context.SoDuQuyCuuTros.Remove(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa bản ghi số dư!";
            }
            return RedirectToAction("SoDu");
        }

        // ==========================================
        // 3. DANH SÁCH ỦNG HỘ
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> DanhSach()
        {
            ViewData["Title"] = "Danh sách ủng hộ Cứu trợ";
            var data = await _context.DanhSachUngHoCuuTros.OrderByDescending(x => x.NgayUngHo).ToListAsync();
            return View("~/Views/Admin/QuyCuuTro/DanhSach.cshtml", data);
        }

        [HttpGet]
        public IActionResult ThemDanhSach() => View("~/Views/Admin/QuyCuuTro/ThemDanhSach.cshtml");

        [HttpPost]
        public async Task<IActionResult> ThemDanhSach(DanhSachUngHoCuuTro model)
        {
            model.HienThi = true;
            _context.DanhSachUngHoCuuTros.Add(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Thêm lượt ủng hộ thành công!";
            return RedirectToAction("DanhSach");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0 || !Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Vui lòng chọn file Excel định dạng .xlsx hợp lệ.";
                return RedirectToAction("DanhSach");
            }

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null) return RedirectToAction("DanhSach");

                    int rowCount = worksheet.Dimension.Rows;
                    int colCount = worksheet.Dimension.Columns;

                    // 1. TỰ ĐỘNG QUÉT TÌM DÒNG TIÊU ĐỀ (Phiên bản V2: Thông minh hơn, chống nhận nhầm)
                    int colTen = 3, colThoiGian = 2, colSoTien = 4; // Vị trí dự phòng
                    int startRow = 8; // Dòng bắt đầu dự phòng

                    for (int r = 1; r <= 15; r++)
                    {
                        int matchCount = 0;
                        int tempColTen = 0, tempColThoiGian = 0, tempColSoTien = 0;

                        for (int c = 1; c <= colCount; c++)
                        {
                            var cellText = worksheet.Cells[r, c].Text?.Trim().ToLower() ?? "";

                            // Gom chung các từ khóa hay dùng
                            if (cellText.Contains("đơn vị") || cellText.Contains("cá nhân") || cellText.Contains("họ tên") || cellText.Contains("tên") || cellText.Contains("cơ quan"))
                            { tempColTen = c; matchCount++; }
                            else if (cellText.Contains("thời gian") || cellText.Contains("ngày ủng hộ") || cellText == "ngày")
                            { tempColThoiGian = c; matchCount++; }
                            else if (cellText.Contains("số tiền") || cellText.Contains("giá trị") || cellText.Contains("ủng hộ"))
                            { tempColSoTien = c; matchCount++; }
                        }

                        // BẢO MẬT KÉP: Nếu 1 dòng có từ 2 cột trở lên khớp từ khóa, chắc chắn 100% đó là dòng tiêu đề!
                        if (matchCount >= 2)
                        {
                            if (tempColTen > 0) colTen = tempColTen;
                            if (tempColThoiGian > 0) colThoiGian = tempColThoiGian;
                            if (tempColSoTien > 0) colSoTien = tempColSoTien;

                            startRow = r + 1; // Dữ liệu sẽ nằm ngay dưới dòng tiêu đề
                            break;
                        }
                    }

                    int successCount = 0;
                    for (int row = startRow; row <= rowCount; row++)
                    {
                        string ten = worksheet.Cells[row, colTen].Text?.Trim();
                        if (string.IsNullOrEmpty(ten)) continue; // Bỏ qua nếu cột tên rỗng

                        // 2. ĐỌC NGÀY THÁNG BỌC THÉP
                        DateTime ngayUngHo = DateTime.Now;
                        try
                        {
                            var cellDate = worksheet.Cells[row, colThoiGian].Value;
                            if (cellDate is DateTime dt) { ngayUngHo = dt; }
                            else if (cellDate is double d) { ngayUngHo = DateTime.FromOADate(d); }
                            else
                            {
                                string dateText = worksheet.Cells[row, colThoiGian].Text?.Trim();
                                if (!string.IsNullOrEmpty(dateText))
                                {
                                    string[] formats = { "dd/MM/yyyy", "d/M/yyyy", "dd/M/yyyy", "d/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-dd" };
                                    if (DateTime.TryParseExact(dateText, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                                        ngayUngHo = parsedDate;
                                    else if (DateTime.TryParse(dateText, out parsedDate))
                                        ngayUngHo = parsedDate;
                                }
                            }
                        }
                        catch { }

                        // 3. ĐỌC SỐ TIỀN BỌC THÉP
                        decimal soTien = 0;
                        try
                        {
                            var cellTien = worksheet.Cells[row, colSoTien].Value;
                            if (cellTien is double d) soTien = (decimal)d;
                            else if (cellTien is decimal dec) soTien = dec;
                            else if (cellTien is int i) soTien = i;
                            else if (cellTien is long l) soTien = l;
                            else
                            {
                                string tienText = worksheet.Cells[row, colSoTien].Text?.Replace(",", "").Replace(".", "").Replace("đ", "").Replace("d", "").Replace(" ", "");
                                decimal.TryParse(tienText, out soTien);
                            }
                        }
                        catch { }

                        if (soTien <= 0) continue;

                        // 4. LOGIC KIỂM TRA TRÙNG VÀ CỘNG DỒN CHO QUỸ CỨU TRỢ
                        var isExistAll = await _context.DanhSachUngHoCuuTros.AnyAsync(x =>
                            x.TenNguoiUngHo == ten &&
                            x.NgayUngHo.HasValue && x.NgayUngHo.Value.Date == ngayUngHo.Date &&
                            x.SoTien == soTien);
                        if (isExistAll) continue;

                        var existingRecord = await _context.DanhSachUngHoCuuTros.FirstOrDefaultAsync(x =>
                            x.TenNguoiUngHo == ten &&
                            x.NgayUngHo.HasValue && x.NgayUngHo.Value.Date == ngayUngHo.Date);

                        if (existingRecord != null)
                        {
                            existingRecord.SoTien += soTien; // Cộng dồn nếu trùng Tên và Ngày
                            _context.Update(existingRecord);
                        }
                        else
                        {
                            var newRecord = new DanhSachUngHoCuuTro
                            {
                                TenNguoiUngHo = ten,
                                NgayUngHo = ngayUngHo,
                                SoTien = soTien,
                                HienThi = true
                            };
                            _context.DanhSachUngHoCuuTros.Add(newRecord);
                        }
                        successCount++;
                    }
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Đã tải lên và tự động xử lý thành công {successCount} dòng dữ liệu!";
                }
            }
            return RedirectToAction("DanhSach");
        }
        [HttpGet]
        public async Task<IActionResult> SuaDanhSach(int id)
        {
            var item = await _context.DanhSachUngHoCuuTros.FindAsync(id);
            if (item == null) return NotFound();
            return View("~/Views/Admin/QuyCuuTro/SuaDanhSach.cshtml", item);
        }

        [HttpPost]
        public async Task<IActionResult> SuaDanhSach(DanhSachUngHoCuuTro model)
        {
            _context.DanhSachUngHoCuuTros.Update(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật lượt ủng hộ thành công!";
            return RedirectToAction("DanhSach");
        }

        [HttpPost]
        public async Task<IActionResult> XoaDanhSach(int id)
        {
            var item = await _context.DanhSachUngHoCuuTros.FindAsync(id);
            if (item != null)
            {
                _context.DanhSachUngHoCuuTros.Remove(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa lượt ủng hộ!";
            }
            return RedirectToAction("DanhSach");
        }

        // ==========================================
        // 4. KẾT QUẢ HOẠT ĐỘNG
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> KetQua()
        {
            ViewData["Title"] = "Hoạt động Cứu trợ";
            var data = await _context.KetQuaHoatDongCuuTros.OrderByDescending(x => x.Nam).ThenByDescending(x => x.Thang).ToListAsync();
            return View("~/Views/Admin/QuyCuuTro/KetQua.cshtml", data);
        }

        [HttpGet]
        public IActionResult ThemKetQua() => View("~/Views/Admin/QuyCuuTro/ThemKetQua.cshtml");

        [HttpPost]
        public async Task<IActionResult> ThemKetQua(KetQuaHoatDongCuuTro model)
        {
            model.TrangThai = true;
            _context.KetQuaHoatDongCuuTros.Add(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Thêm hoạt động thành công!";
            return RedirectToAction("KetQua");
        }

        [HttpGet]
        public async Task<IActionResult> SuaKetQua(int id)
        {
            var item = await _context.KetQuaHoatDongCuuTros.FindAsync(id);
            if (item == null) return NotFound();
            return View("~/Views/Admin/QuyCuuTro/SuaKetQua.cshtml", item);
        }

        [HttpPost]
        public async Task<IActionResult> SuaKetQua(KetQuaHoatDongCuuTro model)
        {
            _context.KetQuaHoatDongCuuTros.Update(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật hoạt động thành công!";
            return RedirectToAction("KetQua");
        }

        [HttpPost]
        public async Task<IActionResult> XoaKetQua(int id)
        {
            var item = await _context.KetQuaHoatDongCuuTros.FindAsync(id);
            if (item != null)
            {
                _context.KetQuaHoatDongCuuTros.Remove(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa hoạt động!";
            }
            return RedirectToAction("KetQua");
        }
    }
}