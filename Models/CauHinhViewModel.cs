namespace WebMTTQ.Models
{
    public class CauHinhViewModel
    {
        // ================================================
        // 1. THÔNG TIN CƠ QUAN (Organization Information)
        // ================================================
        public string? TenCoQuan { get; set; }
        public string? DiaChi { get; set; }
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public string? GioLamViec { get; set; }

        // ================================================
        // 2. LIÊN KẾT MẠNG XÃ HỘI (Social Links)
        // ================================================
        public string? LinkFacebook { get; set; }
        public string? LinkZalo { get; set; }

        // ================================================
        // 3. BẢO TRÌ HỆ THỐNG (Maintenance Mode)
        // ================================================
        public bool BaoTriHeThong { get; set; }

        // ================================================
        // 4. CẤU HÌNH THƯ MỤC (Folder Configuration)
        // ================================================
        public string? Folder_Documents { get; set; }
        public string? Folder_Images { get; set; }
        public string? Folder_Avatars { get; set; }
        public string? Folder_NewsImages { get; set; }
        public string? Folder_TempUpload { get; set; }
        public string? Folder_Backup { get; set; }
        public string? Folder_Export { get; set; }
        public string? Folder_Archive { get; set; }

        // ================================================
        // 6. TỔ CHỨC TÀI LIỆU (Document Organization)
        // ================================================
        public string? DocOrg_SeparateBy { get; set; } // None, Year, Month, Department, DocumentCategory, Combination

        // ================================================
        // 6.1. CẤU HÌNH SMTP EMAIL (Gửi OTP)
        // ================================================
        public string? SmtpHost { get; set; }
        public int SmtpPort { get; set; } = 587;
        public bool SmtpUseSsl { get; set; } = true;
        public string? SmtpUsername { get; set; }
        public string? SmtpPassword { get; set; }
        public string? SmtpFromEmail { get; set; }
        public string? SmtpFromName { get; set; }

        // ================================================
        // 7. QUY TẮC TẢI LÊN (Upload Rules)
        // ================================================
        public long Upload_MaxImageSize { get; set; }          // bytes
        public long Upload_MaxDocumentSize { get; set; }       // bytes
        public long Upload_MaxTotalSize { get; set; }          // bytes
        public string? Upload_AllowedImageExtensions { get; set; }
        public string? Upload_AllowedDocumentExtensions { get; set; }
        public bool Upload_AutoRenameDuplicate { get; set; }
        public bool Upload_KeepOriginalFilename { get; set; }
        public bool Upload_GenerateGUIDFilename { get; set; }
        public bool Upload_GenerateDateFilename { get; set; }
    }
}