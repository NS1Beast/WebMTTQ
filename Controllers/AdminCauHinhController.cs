using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebMTTQ.Models;
using WebMTTQ.Services;

namespace WebMTTQ.Controllers
{
    [Route("AdminCauHinh")]
    [KiemTraQuyen(ModuleQuyen.CauHinh)]
    public class AdminCauHinhController : BaseAdminController
    {
        private readonly ISystemSettingsService _settings;

        public AdminCauHinhController(ISystemSettingsService settings)
        {
            _settings = settings;
        }

        [Route("")]
        [Route("Index")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new CauHinhViewModel
            {
                // --- Organization Information ---
                TenCoQuan = await _settings.GetValueAsync("TenCoQuan"),
                DiaChi = await _settings.GetValueAsync("DiaChi"),
                SoDienThoai = await _settings.GetValueAsync("SoDienThoai"),
                Email = await _settings.GetValueAsync("Email"),
                GioLamViec = await _settings.GetValueAsync("GioLamViec"),

                // --- Social Links ---
                LinkFacebook = await _settings.GetValueAsync("LinkFacebook"),
                LinkZalo = await _settings.GetValueAsync("LinkZalo"),

                // --- Maintenance Mode ---
                BaoTriHeThong = await _settings.GetBooleanAsync("BaoTriHeThong"),

                // --- Folder Configuration ---
                Folder_Documents = await _settings.GetValueAsync("Folder_Documents"),
                Folder_Images = await _settings.GetValueAsync("Folder_Images"),
                Folder_Avatars = await _settings.GetValueAsync("Folder_Avatars"),
                Folder_NewsImages = await _settings.GetValueAsync("Folder_NewsImages"),
                Folder_TempUpload = await _settings.GetValueAsync("Folder_TempUpload"),
                Folder_Backup = await _settings.GetValueAsync("Folder_Backup"),
                Folder_Export = await _settings.GetValueAsync("Folder_Export"),
                Folder_Archive = await _settings.GetValueAsync("Folder_Archive"),

                // --- Document Organization ---
                DocOrg_SeparateBy = await _settings.GetValueAsync("DocOrg_SeparateBy"),

                // --- SMTP Email ---
                SmtpHost = await _settings.GetValueAsync("SmtpHost"),
                SmtpPort = await _settings.GetIntAsync("SmtpPort"),
                SmtpUseSsl = await _settings.GetBooleanAsync("SmtpUseSsl"),
                SmtpUsername = await _settings.GetValueAsync("SmtpUsername"),
                SmtpPassword_Display = await _settings.ExistsAsync("SmtpPassword") ? "**************" : string.Empty,
                SmtpFromEmail = await _settings.GetValueAsync("SmtpFromEmail"),
                SmtpFromName = await _settings.GetValueAsync("SmtpFromName"),

                // --- Upload Rules ---
                Upload_MaxImageSize = await _settings.GetLongAsync("Upload_MaxImageSize"),
                Upload_MaxDocumentSize = await _settings.GetLongAsync("Upload_MaxDocumentSize"),
                Upload_MaxTotalSize = await _settings.GetLongAsync("Upload_MaxTotalSize"),
                Upload_AllowedImageExtensions = await _settings.GetValueAsync("Upload_AllowedImageExtensions"),
                Upload_AllowedDocumentExtensions = await _settings.GetValueAsync("Upload_AllowedDocumentExtensions"),
                Upload_AutoRenameDuplicate = await _settings.GetBooleanAsync("Upload_AutoRenameDuplicate"),
                Upload_KeepOriginalFilename = await _settings.GetBooleanAsync("Upload_KeepOriginalFilename"),
                Upload_GenerateGUIDFilename = await _settings.GetBooleanAsync("Upload_GenerateGUIDFilename"),
                Upload_GenerateDateFilename = await _settings.GetBooleanAsync("Upload_GenerateDateFilename")
            };

            return View("~/Views/Admin/CauHinh/Index.cshtml", model);
        }

        [Route("")]
        [Route("Index")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CauHinhViewModel model)
        {
            if (ModelState.IsValid)
            {
                // --- Organization Information ---
                await _settings.SetValueAsync("TenCoQuan", model.TenCoQuan, "Tên cơ quan / tổ chức");
                await _settings.SetValueAsync("DiaChi", model.DiaChi, "Địa chỉ trụ sở");
                await _settings.SetValueAsync("SoDienThoai", model.SoDienThoai, "Số điện thoại liên hệ đường dây nóng");
                await _settings.SetValueAsync("Email", model.Email, "Hộp thư điện tử tiếp nhận");
                await _settings.SetValueAsync("GioLamViec", model.GioLamViec, "Giờ làm việc hành chính");

                // --- Social Links ---
                await _settings.SetValueAsync("LinkFacebook", model.LinkFacebook, "Đường dẫn Fanpage Facebook");
                await _settings.SetValueAsync("LinkZalo", model.LinkZalo, "Đường dẫn Zalo OA");

                // --- Maintenance Mode ---
                await _settings.SetValueAsync("BaoTriHeThong", model.BaoTriHeThong ? "1" : "0", "Chế độ bảo trì hệ thống (1=Bật, 0=Tắt)");

                // --- Folder Configuration ---
                await _settings.SetValueAsync("Folder_Documents", model.Folder_Documents, "Thư mục lưu tài liệu");
                await _settings.SetValueAsync("Folder_Images", model.Folder_Images, "Thư mục lưu hình ảnh");
                await _settings.SetValueAsync("Folder_Avatars", model.Folder_Avatars, "Thư mục lưu avatar");
                await _settings.SetValueAsync("Folder_NewsImages", model.Folder_NewsImages, "Thư mục lưu ảnh tin tức");
                await _settings.SetValueAsync("Folder_TempUpload", model.Folder_TempUpload, "Thư mục tải lên tạm thời");
                await _settings.SetValueAsync("Folder_Backup", model.Folder_Backup, "Thư mục sao lưu");
                await _settings.SetValueAsync("Folder_Export", model.Folder_Export, "Thư mục xuất dữ liệu");
                await _settings.SetValueAsync("Folder_Archive", model.Folder_Archive, "Thư mục lưu trữ");

                // --- Document Organization ---
                await _settings.SetValueAsync("DocOrg_SeparateBy", model.DocOrg_SeparateBy, "Phân loại thư mục theo (None, Year, Month, Department, DocumentCategory, Combination)");

                // --- SMTP Email ---
                await _settings.SetValueAsync("SmtpHost", model.SmtpHost, "SMTP Host để gửi email OTP");
                await _settings.SetValueAsync("SmtpPort", model.SmtpPort.ToString(), "SMTP Port");
                await _settings.SetValueAsync("SmtpUseSsl", model.SmtpUseSsl ? "1" : "0", "Bật SSL/TLS cho SMTP");
                await _settings.SetValueAsync("SmtpUsername", model.SmtpUsername, "SMTP Username");
                if (!string.IsNullOrEmpty(model.SmtpPassword))
                {
                    await _settings.SetEncryptedValueAsync("SmtpPassword", model.SmtpPassword, "SMTP Password (mã hóa)");
                }
                await _settings.SetValueAsync("SmtpFromEmail", model.SmtpFromEmail, "Email gửi (From)");
                await _settings.SetValueAsync("SmtpFromName", model.SmtpFromName, "Tên hiển thị (From)");

                // --- Upload Rules ---
                await _settings.SetValueAsync("Upload_MaxImageSize", model.Upload_MaxImageSize.ToString(), "Kích thước tối đa tập tin hình ảnh (bytes)");
                await _settings.SetValueAsync("Upload_MaxDocumentSize", model.Upload_MaxDocumentSize.ToString(), "Kích thước tối đa tập tin tài liệu (bytes)");
                await _settings.SetValueAsync("Upload_MaxTotalSize", model.Upload_MaxTotalSize.ToString(), "Tổng kích thước tải lên tối đa (bytes)");
                await _settings.SetValueAsync("Upload_AllowedImageExtensions", model.Upload_AllowedImageExtensions, "Định dạng hình ảnh cho phép (VD: .jpg,.png,.gif)");
                await _settings.SetValueAsync("Upload_AllowedDocumentExtensions", model.Upload_AllowedDocumentExtensions, "Định dạng tài liệu cho phép (VD: .pdf,.docx,.xlsx)");
                await _settings.SetValueAsync("Upload_AutoRenameDuplicate", model.Upload_AutoRenameDuplicate ? "1" : "0", "Tự động đổi tên khi trùng lặp");
                await _settings.SetValueAsync("Upload_KeepOriginalFilename", model.Upload_KeepOriginalFilename ? "1" : "0", "Giữ nguyên tên tập tin gốc");
                await _settings.SetValueAsync("Upload_GenerateGUIDFilename", model.Upload_GenerateGUIDFilename ? "1" : "0", "Tạo tên tập tin theo GUID");
                await _settings.SetValueAsync("Upload_GenerateDateFilename", model.Upload_GenerateDateFilename ? "1" : "0", "Tạo tên tập tin theo ngày tháng");

                TempData["SuccessMessage"] = "Đã lưu tất cả cài đặt hệ thống thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View("~/Views/Admin/CauHinh/Index.cshtml", model);
        }
    }
}