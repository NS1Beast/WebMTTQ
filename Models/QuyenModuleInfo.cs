namespace WebMTTQ.Models
{
    /// <summary>
    /// Thông tin quyền truy cập của một module.
    /// </summary>
    public class ModuleQuyenInfo
    {
        public string MaModule { get; set; } = string.Empty;
        public string TenModule { get; set; } = string.Empty;
        public bool CoQuyenXem { get; set; }
        public bool CoQuyenThem { get; set; }
        public bool CoQuyenSua { get; set; }
        public bool CoQuyenXoa { get; set; }
    }

    /// <summary>
    /// Quyền truy cập được lưu dạng bitmask (tinyint) trong VaiTro.QuyenHan:
    /// 1 = Xem, 2 = Thêm, 4 = Sửa, 8 = Xóa.
    /// Admin (toàn quyền) = 15 (1+2+4+8).
    /// </summary>
    public static class QuyenBitmask
    {
        public const byte Xem = 1;
        public const byte Them = 2;
        public const byte Sua = 4;
        public const byte Xoa = 8;
        public const byte ToanQuyen = 15; // 1+2+4+8

        public static bool CoQuyenXem(byte? quyenHan) => ((quyenHan ?? 0) & Xem) == Xem;
        public static bool CoQuyenThem(byte? quyenHan) => ((quyenHan ?? 0) & Them) == Them;
        public static bool CoQuyenSua(byte? quyenHan) => ((quyenHan ?? 0) & Sua) == Sua;
        public static bool CoQuyenXoa(byte? quyenHan) => ((quyenHan ?? 0) & Xoa) == Xoa;

        public static byte BuildQuyenHan(bool coQuyenXem, bool coQuyenThem, bool coQuyenSua, bool coQuyenXoa)
        {
            byte result = 0;
            if (coQuyenXem) result |= Xem;
            if (coQuyenThem) result |= Them;
            if (coQuyenSua) result |= Sua;
            if (coQuyenXoa) result |= Xoa;
            return result;
        }
    }

    /// <summary>
    /// Helper class để kiểm tra quyền từ VaiTro.QuyenHan (bitmask tinyint).
    /// </summary>
    public static class QuyenHelper
    {
        /// <summary>
        /// Kiểm tra một VaiTro có phải là Admin hệ thống không (toàn quyền).
        /// Chỉ khớp CHÍNH XÁC tên vai trò "Quản trị viên" hoặc "Admin" (không phân biệt hoa thường).
        /// KHÔNG dùng Contains vì sẽ khiến các vai trò khác có tên chứa "Admin"/"Quản trị"
        /// (vd: "Quản trị nội dung", "Biên tập viên Admin") bị nhầm thành vai trò Admin hệ thống.
        /// </summary>
        public static bool IsAdminVaiTro(string? tenVaiTro)
        {
            if (string.IsNullOrEmpty(tenVaiTro)) return false;

            var normalized = tenVaiTro.Trim();
            return normalized.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                   normalized.Equals("Quản trị viên", StringComparison.OrdinalIgnoreCase) ||
                   normalized.Equals("Quan tri vien", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Kiểm tra một VaiTro có toàn quyền (bitmask = 15) hay không.
        /// </summary>
        public static bool IsToanQuyen(byte? quyenHan)
        {
            return quyenHan == QuyenBitmask.ToanQuyen;
        }
    }
}