using Microsoft.EntityFrameworkCore;
using WebMTTQ.Models;

namespace WebMTTQ.Services
{
    /// <summary>
    /// Seeds the CauHinhHeThong table with default configuration keys
    /// if they do not already exist. This ensures backward compatibility
    /// and provides sensible defaults for new settings.
    /// </summary>
    public static class SystemSettingsSeeder
    {
        /// <summary>
        /// Ensures all required configuration keys exist in the database.
        /// Call this method at application startup.
        /// </summary>
        public static async Task SeedAsync(DataMTTQContext context)
        {
            var existingKeys = await context.CauHinhHeThongs
                .Select(c => c.MaCauHinh)
                .ToListAsync();

            // Helper to add a key if it doesn't exist
            void AddIfNotExists(string key, string defaultValue, string description)
            {
                if (!existingKeys.Contains(key))
                {
                    context.CauHinhHeThongs.Add(new CauHinhHeThong
                    {
                        MaCauHinh = key,
                        GiaTriCauHinh = defaultValue,
                        MoTa = description
                    });
                }
            }

            // ================================================
            // EXISTING KEYS (already in database, but ensure they exist)
            // ================================================
            AddIfNotExists("TenCoQuan", "", "Tên cơ quan / tổ chức");
            AddIfNotExists("DiaChi", "", "Địa chỉ trụ sở");
            AddIfNotExists("SoDienThoai", "", "Số điện thoại liên hệ đường dây nóng");
            AddIfNotExists("Email", "", "Hộp thư điện tử tiếp nhận");
            AddIfNotExists("GioLamViec", "", "Giờ làm việc hành chính");
            AddIfNotExists("LinkFacebook", "", "Đường dẫn Fanpage Facebook");
            AddIfNotExists("LinkZalo", "", "Đường dẫn Zalo OA");
            AddIfNotExists("BaoTriHeThong", "0", "Chế độ bảo trì hệ thống (1=Bật, 0=Tắt)");

            // ================================================
            // FOLDER CONFIGURATION
            // ================================================
            AddIfNotExists("Folder_Documents", "documents/", "Thư mục lưu tài liệu");
            AddIfNotExists("Folder_Images", "images/", "Thư mục lưu hình ảnh");
            AddIfNotExists("Folder_Avatars", "avatars/", "Thư mục lưu avatar");
            AddIfNotExists("Folder_NewsImages", "news/", "Thư mục lưu ảnh tin tức");
            AddIfNotExists("Folder_TempUpload", "temp/", "Thư mục tải lên tạm thời");
            AddIfNotExists("Folder_Backup", "backup/", "Thư mục sao lưu");
            AddIfNotExists("Folder_Export", "export/", "Thư mục xuất dữ liệu");
            AddIfNotExists("Folder_Archive", "archive/", "Thư mục lưu trữ");

            // ================================================
            // DOCUMENT ORGANIZATION
            // ================================================
            AddIfNotExists("DocOrg_SeparateBy", "None", "Phân loại thư mục theo (None, Year, Month, Department, DocumentCategory, Combination)");

            // ================================================
            // SMTP EMAIL (Gửi OTP)
            // ================================================
            AddIfNotExists("SmtpHost", "", "SMTP Host để gửi email OTP");
            AddIfNotExists("SmtpPort", "587", "SMTP Port");
            AddIfNotExists("SmtpUseSsl", "1", "Bật SSL/TLS cho SMTP");
            AddIfNotExists("SmtpUsername", "", "SMTP Username");
            AddIfNotExists("SmtpPassword", "", "SMTP Password (mã hóa)");
            AddIfNotExists("SmtpFromEmail", "", "Email gửi (From)");
            AddIfNotExists("SmtpFromName", "MTTQ Phường Tân Định", "Tên hiển thị (From)");

            // ================================================
            // UPLOAD RULES
            // ================================================
            AddIfNotExists("Upload_MaxImageSize", "5242880", "Kích thước tối đa tập tin hình ảnh (bytes) - Mặc định 5MB");
            AddIfNotExists("Upload_MaxDocumentSize", "10485760", "Kích thước tối đa tập tin tài liệu (bytes) - Mặc định 10MB");
            AddIfNotExists("Upload_MaxTotalSize", "104857600", "Tổng kích thước tải lên tối đa (bytes) - Mặc định 100MB");
            AddIfNotExists("Upload_AllowedImageExtensions", ".jpg,.jpeg,.png,.gif,.webp", "Định dạng hình ảnh cho phép");
            AddIfNotExists("Upload_AllowedDocumentExtensions", ".pdf,.docx,.xlsx,.pptx", "Định dạng tài liệu cho phép");
            AddIfNotExists("Upload_AutoRenameDuplicate", "1", "Tự động đổi tên khi trùng lặp");
            AddIfNotExists("Upload_KeepOriginalFilename", "0", "Giữ nguyên tên tập tin gốc");
            AddIfNotExists("Upload_GenerateGUIDFilename", "0", "Tạo tên tập tin theo GUID");
            AddIfNotExists("Upload_GenerateDateFilename", "1", "Tạo tên tập tin theo ngày tháng");

            await context.SaveChangesAsync();
        }
    }
}