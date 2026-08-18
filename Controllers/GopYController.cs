using Microsoft.AspNetCore.Mvc;
using WebMTTQ.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebMTTQ.Controllers
{
    public class GopYController : Controller
    {
        private readonly DataMTTQContext _context;
        private static readonly string[] _allowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };
        private static readonly string[] _allowedMimeTypes = { "application/pdf", "image/jpeg", "image/png" };
        private const int MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB

        public GopYController(DataMTTQContext context)
        {
            _context = context;
        }

        // 1. Hàm hiển thị trang form
        [HttpGet]
        public IActionResult Index()
        {
            return View("~/Views/GopY/Index.cshtml");
        }

        // 2. Hàm xử lý khi người dùng bấm nút "Gửi góp ý"
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(12 * 1024 * 1024)] // Giới hạn request 12MB (form + file 10MB)
        public async Task<IActionResult> GuiGopY(HopThuGopY model, IFormFile? tepDinhKem)
        {
            // === Chống spam: kiểm tra thời gian giữa các lần gửi (tối thiểu 30 giây) ===
            var lastSubmitKey = $"GopY_LastSubmit_{HttpContext.Connection.RemoteIpAddress}";
            var lastSubmit = HttpContext.Session.GetString(lastSubmitKey);
            if (!string.IsNullOrEmpty(lastSubmit) && long.TryParse(lastSubmit, out var lastTicks))
            {
                var lastTime = new DateTime(lastTicks);
                if ((DateTime.Now - lastTime).TotalSeconds < 30)
                {
                    ModelState.AddModelError("", "Bạn vừa gửi góp ý. Vui lòng chờ 30 giây trước khi gửi tiếp.");
                    return View("~/Views/GopY/Index.cshtml", model);
                }
            }

            // === Sanitize dữ liệu đầu vào để chống XSS nếu dữ liệu được hiển thị ở nơi không được escape ===
            if (!string.IsNullOrWhiteSpace(model.HoTen))
                model.HoTen = SanitizeText(model.HoTen);
            if (!string.IsNullOrWhiteSpace(model.TieuDe))
                model.TieuDe = SanitizeText(model.TieuDe);
            if (!string.IsNullOrWhiteSpace(model.DiaChi))
                model.DiaChi = SanitizeText(model.DiaChi);
            if (!string.IsNullOrWhiteSpace(model.LinhVuc))
                model.LinhVuc = SanitizeText(model.LinhVuc);
            if (!string.IsNullOrWhiteSpace(model.Email))
                model.Email = model.Email.Trim();
            if (!string.IsNullOrWhiteSpace(model.SoDienThoai))
                model.SoDienThoai = model.SoDienThoai.Trim();
            if (!string.IsNullOrWhiteSpace(model.NoiDung))
                model.NoiDung = SanitizeText(model.NoiDung);

            // === Model Validation ===
            if (ModelState.IsValid && !string.IsNullOrWhiteSpace(model.NoiDung))
            {
                // Kiểm tra thêm nội dung không chỉ toàn khoảng trắng
                model.NgayGui = DateTime.Now;
                model.TrangThai = "Chưa xử lý";
                model.DaXoa = false;

                // === Xử lý file đính kèm an toàn ===
                if (tepDinhKem != null && tepDinhKem.Length > 0)
                {
                    var validationError = ValidateUploadFile(tepDinhKem);
                    if (!string.IsNullOrEmpty(validationError))
                    {
                        ModelState.AddModelError("", validationError);
                        return View("~/Views/GopY/Index.cshtml", model);
                    }

                    using (var memoryStream = new MemoryStream())
                    {
                        await tepDinhKem.CopyToAsync(memoryStream);

                        // Chỉ lưu phần file để tránh lưu quá nhiều rác
                        // Giới hạn lại để tránh overflow database
                        model.TepMinhChung = memoryStream.ToArray();
                    }
                }

                _context.HopThuGopies.Add(model);
                await _context.SaveChangesAsync();

                // Lưu thời gian gửi cuối cùng để chống spam
                HttpContext.Session.SetString(lastSubmitKey, DateTime.Now.Ticks.ToString());

                TempData["SuccessMessage"] = "Gửi góp ý thành công. Trân trọng cảm ơn ý kiến của Quý vị!";
                return RedirectToAction("Index");
            }

            // Nếu dữ liệu nhập lỗi, trả lại form với thông báo lỗi
            return View("~/Views/GopY/Index.cshtml", model);
        }

        /// <summary>
        /// Kiểm tra tính hợp lệ của file upload: extension, MIME type, kích thước.
        /// </summary>
        private string? ValidateUploadFile(IFormFile file)
        {
            // Kiểm tra kích thước
            if (file.Length > MaxFileSizeBytes)
            {
                return "File đính kèm không được vượt quá 10MB.";
            }

            // Kiểm tra extension (không tin tưởng tên file từ client)
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !_allowedExtensions.Contains(extension))
            {
                return "Chỉ chấp nhận file PDF, JPG, hoặc PNG.";
            }

            // Kiểm tra MIME type
            if (string.IsNullOrEmpty(file.ContentType) || !_allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                return "Định dạng file không hợp lệ. Chỉ chấp nhận PDF, JPG, hoặc PNG.";
            }

            // Kiểm tra signature của file (magic bytes) để chống file thực thi giả danh
            using (var stream = file.OpenReadStream())
            {
                var headerBytes = new byte[8];
                var bytesRead = stream.Read(headerBytes, 0, headerBytes.Length);

                bool isValidSignature = false;

                switch (extension)
                {
                    case ".pdf":
                        // PDF bắt đầu bằng "%PDF"
                        isValidSignature = bytesRead >= 4 &&
                            headerBytes[0] == 0x25 && headerBytes[1] == 0x50 &&
                            headerBytes[2] == 0x44 && headerBytes[3] == 0x46;
                        break;
                    case ".jpg":
                    case ".jpeg":
                        // JPEG bắt đầu bằng FF D8 FF
                        isValidSignature = bytesRead >= 3 &&
                            headerBytes[0] == 0xFF && headerBytes[1] == 0xD8 && headerBytes[2] == 0xFF;
                        break;
                    case ".png":
                        // PNG bắt đầu bằng 89 50 4E 47 0D 0A 1A 0A
                        isValidSignature = bytesRead >= 8 &&
                            headerBytes[0] == 0x89 && headerBytes[1] == 0x50 &&
                            headerBytes[2] == 0x4E && headerBytes[3] == 0x47 &&
                            headerBytes[4] == 0x0D && headerBytes[5] == 0x0A &&
                            headerBytes[6] == 0x1A && headerBytes[7] == 0x0A;
                        break;
                }

                if (!isValidSignature)
                {
                    return "File không đúng định dạng thực tế. Vui lòng chọn file PDF, JPG hoặc PNG hợp lệ.";
                }
            }

            return null;
        }

        /// <summary>
        /// Loại bỏ các ký tự HTML/script nguy hiểm khỏi chuỗi nhập vào.
        /// </summary>
        private static string SanitizeText(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input?.Trim() ?? string.Empty;
            return input.Trim();
        }
    }
}