using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebMTTQ.Models;
using System.Threading.Tasks;
using System.Linq;
using OfficeOpenXml;

namespace WebMTTQ.Controllers
{
    [Route("admin/danhsachungho")]
    [KiemTraQuyen(ModuleQuyen.DanhSachUngHo)]
    public class AdminDanhSachUngHoController : BaseAdminController
    {
        private readonly DataMTTQContext _context;

        public AdminDanhSachUngHoController(DataMTTQContext context)
        {
            _context = context;
        }

        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            // Sắp xếp ngày mới nhất lên đầu
            var list = await _context.DanhSachUngHos.AsNoTracking().OrderByDescending(x => x.NgayUngHo).ToListAsync();
            return View("~/Views/Admin/DanhSachUngHo/Index.cshtml", list);
        }

        [Route("Create")]
        public IActionResult Create()
        {
            return View("~/Views/Admin/DanhSachUngHo/Create.cshtml");
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DanhSachUngHo model)
        {
            if (ModelState.IsValid)
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
                // THÊM DÒNG NÀY TRƯỚC KHI RETURN
                TempData["SuccessMessage"] = "Thêm danh sách ủng hộ thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Admin/DanhSachUngHo/Create.cshtml", model);
        }
        [HttpPost("ImportExcel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0 || !Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Vui lòng chọn file Excel định dạng .xlsx hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null) return RedirectToAction(nameof(Index));

                    int rowCount = worksheet.Dimension.Rows;
                    int colCount = worksheet.Dimension.Columns;

                    // 1. TỰ ĐỘNG QUÉT TÌM DÒNG TIÊU ĐỀ (Phiên bản V2: Thông minh hơn, chống nhận nhầm dòng giới thiệu)
                    int colTen = 3, colThoiGian = 2, colSoTien = 4; // Vị trí dự phòng
                    int startRow = 8; // Dòng bắt đầu dự phòng

                    for (int r = 1; r <= 15; r++)
                    {
                        int matchCount = 0;
                        int tempColTen = 0, tempColThoiGian = 0, tempColSoTien = 0;

                        for (int c = 1; c <= colCount; c++)
                        {
                            var cellText = worksheet.Cells[r, c].Text?.Trim().ToLower() ?? "";

                            // Quét các từ khóa
                            if (cellText.Contains("đơn vị") || cellText.Contains("cá nhân") || cellText.Contains("họ tên") || cellText.Contains("tên") || cellText.Contains("cơ quan"))
                            { tempColTen = c; matchCount++; }
                            else if (cellText.Contains("thời gian") || cellText.Contains("ngày ủng hộ") || cellText == "ngày")
                            { tempColThoiGian = c; matchCount++; }
                            else if (cellText.Contains("số tiền") || cellText.Contains("giá trị") || cellText.Contains("ủng hộ"))
                            { tempColSoTien = c; matchCount++; }
                        }

                        // BẢO MẬT KÉP: Yêu cầu ít nhất 2 từ khóa khớp mới xác nhận đó là dòng tiêu đề thực sự!
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
                        string? ten = worksheet.Cells[row, colTen].Text?.Trim();
                        // Bỏ qua nếu cột tên trống (Ví dụ như dòng 8: "Từ ngày 01/04..." nhưng cột Tên lại rỗng)
                        if (string.IsNullOrEmpty(ten)) continue;

                        // 2. ĐỌC NGÀY THÁNG BỌC THÉP
                        DateTime ngayUngHo = DateTime.Now;
                        try
                        {
                            var cellDate = worksheet.Cells[row, colThoiGian].Value;
                            if (cellDate is DateTime dt)
                            {
                                ngayUngHo = dt;
                            }
                            else if (cellDate is double d)
                            {
                                ngayUngHo = DateTime.FromOADate(d);
                            }
                            else
                            {
                                string? dateText = worksheet.Cells[row, colThoiGian].Text?.Trim();
                                if (!string.IsNullOrEmpty(dateText))
                                {
                                    // Thử ép kiểu ngày tháng dạng phổ biến của Việt Nam
                                    string[] formats = { "dd/MM/yyyy", "d/M/yyyy", "dd/M/yyyy", "d/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-dd" };
                                    if (DateTime.TryParseExact(dateText, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                                    {
                                        ngayUngHo = parsedDate;
                                    }
                                    else if (DateTime.TryParse(dateText, out parsedDate))
                                    {
                                        ngayUngHo = parsedDate;
                                    }
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
                                string? tienText = worksheet.Cells[row, colSoTien].Text?.Replace(",", "").Replace(".", "").Replace("đ", "").Replace("d", "").Replace(" ", "");
                                decimal.TryParse(tienText, out soTien);
                            }
                        }
                        catch { }

                        if (soTien <= 0) continue; // Bỏ qua nếu dòng đó không có tiền ủng hộ

                        // 4. LOGIC KIỂM TRA TRÙNG VÀ CỘNG DỒN CHO QUỸ VÌ NGƯỜI NGHÈO
                        // (Cột NgayUngHo bảng này là DateTime bắt buộc nên so sánh trực tiếp Date)
                        var isExistAll = await _context.DanhSachUngHos.AnyAsync(x =>
                            x.TenNguoiUngHo == ten &&
                            x.NgayUngHo.Date == ngayUngHo.Date &&
                            x.SoTien == soTien);

                        if (isExistAll) continue;

                        var existingRecord = await _context.DanhSachUngHos.FirstOrDefaultAsync(x =>
                            x.TenNguoiUngHo == ten &&
                            x.NgayUngHo.Date == ngayUngHo.Date);

                        if (existingRecord != null)
                        {
                            existingRecord.SoTien += soTien; // Cộng dồn số tiền
                            _context.Update(existingRecord);
                        }
                        else
                        {
                            var newRecord = new DanhSachUngHo
                            {
                                TenNguoiUngHo = ten,
                                NgayUngHo = ngayUngHo,
                                SoTien = soTien
                                // Xóa hoặc bật dòng dưới (HienThi = true) tùy thiết kế Database quỹ này của bạn
                            };
                            _context.DanhSachUngHos.Add(newRecord);
                        }
                        successCount++;
                    }
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Đã tải lên và tự động xử lý thành công {successCount} dòng dữ liệu!";
                }
            }
            return RedirectToAction(nameof(Index));
        }
        // --- BẮT ĐẦU PHẦN CODE SỬA ---

        [Route("Edit/{id?}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            // Tìm record trong database theo Id
            var item = await _context.DanhSachUngHos.FindAsync(id);
            if (item == null) return NotFound();

            return View("~/Views/Admin/DanhSachUngHo/Edit.cshtml", item);
        }

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DanhSachUngHo model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                    // THÊM DÒNG NÀY SAU KHI LƯU THÀNH CÔNG
                    TempData["SuccessMessage"] = "Cập nhật danh sách ủng hộ thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.DanhSachUngHos.Any(e => e.Id == model.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Admin/DanhSachUngHo/Edit.cshtml", model);
        }
        // --- KẾT THÚC PHẦN CODE SỬA ---
        // Action Xóa
        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.DanhSachUngHos.FindAsync(id);
            if (item != null)
            {
                _context.DanhSachUngHos.Remove(item);
                await _context.SaveChangesAsync();
                // THÊM DÒNG NÀY VÀO TRONG KHỐI LỆNH IF
                TempData["SuccessMessage"] = "Đã xóa bản ghi thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}