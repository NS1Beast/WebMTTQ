using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebMTTQ.Migrations
{
    /// <inheritdoc />
    public partial class SplitChuyenMucVanBanTaiLieu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ============================================================
            // 1. Phân loại các chuyên mục hiện có (chưa có LoaiChuyenMuc)
            //    thành chuyên mục Tin tức (mặc định)
            // ============================================================
            migrationBuilder.Sql(@"
                UPDATE ChuyenMuc
                SET LoaiChuyenMuc = 'TinTuc'
                WHERE LoaiChuyenMuc IS NULL OR LoaiChuyenMuc = '';
            ");

            // ============================================================
            // 2. Seed dữ liệu chuyên mục Văn bản tài liệu mặc định
            // ============================================================
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM ChuyenMuc WHERE DuongDan = 'van-ban-chi-dao' AND LoaiChuyenMuc = 'VanBanTaiLieu')
                BEGIN
                    INSERT INTO ChuyenMuc (TenChuyenMuc, DuongDan, LoaiChuyenMuc, ThuTu, DaXoa, HienThi)
                    VALUES (N'Văn bản chỉ đạo', 'van-ban-chi-dao', 'VanBanTaiLieu', 1, 0, 1);
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM ChuyenMuc WHERE DuongDan = 'nghi-quyet' AND LoaiChuyenMuc = 'VanBanTaiLieu')
                BEGIN
                    INSERT INTO ChuyenMuc (TenChuyenMuc, DuongDan, LoaiChuyenMuc, ThuTu, DaXoa, HienThi)
                    VALUES (N'Nghị quyết', 'nghi-quyet', 'VanBanTaiLieu', 2, 0, 1);
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM ChuyenMuc WHERE DuongDan = 'quyet-dinh' AND LoaiChuyenMuc = 'VanBanTaiLieu')
                BEGIN
                    INSERT INTO ChuyenMuc (TenChuyenMuc, DuongDan, LoaiChuyenMuc, ThuTu, DaXoa, HienThi)
                    VALUES (N'Quyết định', 'quyet-dinh', 'VanBanTaiLieu', 3, 0, 1);
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM ChuyenMuc WHERE DuongDan = 'ke-hoach' AND LoaiChuyenMuc = 'VanBanTaiLieu')
                BEGIN
                    INSERT INTO ChuyenMuc (TenChuyenMuc, DuongDan, LoaiChuyenMuc, ThuTu, DaXoa, HienThi)
                    VALUES (N'Kế hoạch', 'ke-hoach', 'VanBanTaiLieu', 4, 0, 1);
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM ChuyenMuc WHERE DuongDan = 'bieu-mau' AND LoaiChuyenMuc = 'VanBanTaiLieu')
                BEGIN
                    INSERT INTO ChuyenMuc (TenChuyenMuc, DuongDan, LoaiChuyenMuc, ThuTu, DaXoa, HienThi)
                    VALUES (N'Biểu mẫu', 'bieu-mau', 'VanBanTaiLieu', 5, 0, 1);
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM ChuyenMuc WHERE DuongDan = 'bao-cao' AND LoaiChuyenMuc = 'VanBanTaiLieu')
                BEGIN
                    INSERT INTO ChuyenMuc (TenChuyenMuc, DuongDan, LoaiChuyenMuc, ThuTu, DaXoa, HienThi)
                    VALUES (N'Báo cáo', 'bao-cao', 'VanBanTaiLieu', 6, 0, 1);
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM ChuyenMuc WHERE DuongDan = 'thong-bao' AND LoaiChuyenMuc = 'VanBanTaiLieu')
                BEGIN
                    INSERT INTO ChuyenMuc (TenChuyenMuc, DuongDan, LoaiChuyenMuc, ThuTu, DaXoa, HienThi)
                    VALUES (N'Thông báo', 'thong-bao', 'VanBanTaiLieu', 7, 0, 1);
                END
            ");

            // ============================================================
            // 3. Cập nhật các văn bản tài liệu hiện có:
            //    Nếu chưa có chuyên mục, gán vào chuyên mục "Văn bản chỉ đạo"
            // ============================================================
            migrationBuilder.Sql(@"
                UPDATE VanBanTaiLieu
                SET IDChuyenMuc = (SELECT TOP 1 IDChuyenMuc FROM ChuyenMuc WHERE DuongDan = 'van-ban-chi-dao' AND LoaiChuyenMuc = 'VanBanTaiLieu')
                WHERE IDChuyenMuc IS NULL AND DaXoa != 1;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Xóa các chuyên mục văn bản tài liệu đã seed
            migrationBuilder.Sql(@"
                DELETE FROM ChuyenMuc
                WHERE LoaiChuyenMuc = 'VanBanTaiLieu'
                  AND DuongDan IN ('van-ban-chi-dao', 'nghi-quyet', 'quyet-dinh', 'ke-hoach', 'bieu-mau', 'bao-cao', 'thong-bao');
            ");

            // Đặt lại LoaiChuyenMuc về NULL cho các chuyên mục tin tức
            migrationBuilder.Sql(@"
                UPDATE ChuyenMuc
                SET LoaiChuyenMuc = NULL
                WHERE LoaiChuyenMuc = 'TinTuc';
            ");
        }
    }
}