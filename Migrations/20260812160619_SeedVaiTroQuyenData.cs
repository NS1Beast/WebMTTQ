using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebMTTQ.Migrations
{
    /// <inheritdoc />
    public partial class SeedVaiTroQuyenData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Seed quyền chi tiết theo module cho từng vai trò hiện có.
            // Với mỗi vai trò, nếu QuyenHan > 0 thì cấp quyền cho TẤT CẢ module
            // theo mức quyền tương ứng trong bitmask (1=Xem, 2=Thêm, 4=Sửa, 8=Xóa).
            // Nếu QuyenHan = 15 (ToanQuyen) thì toàn quyền tất cả modules.
            migrationBuilder.Sql(@"
-- Seed cho Admin (QuyenHan = 15): toàn quyền tất cả modules
INSERT INTO VaiTroQuyen (IDVaiTro, MaModule, CoQuyenXem, CoQuyenThem, CoQuyenSua, CoQuyenXoa)
SELECT v.IDVaiTro, m.MaModule, 1, 1, 1, 1
FROM VaiTro v
CROSS JOIN (
    SELECT 'dashboard' AS MaModule UNION ALL
    SELECT 'trangchu' UNION ALL
    SELECT 'thongtinungho' UNION ALL
    SELECT 'danhsachungho' UNION ALL
    SELECT 'soduquy' UNION ALL
    SELECT 'ketquachamlo' UNION ALL
    SELECT 'diadiembando' UNION ALL
    SELECT 'nguoidancantrogium' UNION ALL
    SELECT 'gopy' UNION ALL
    SELECT 'banner' UNION ALL
    SELECT 'cauhinh' UNION ALL
    SELECT 'quanlynguoidung'
) m
WHERE v.QuyenHan = 15
  AND NOT EXISTS (
      SELECT 1 FROM VaiTroQuyen q 
      WHERE q.IDVaiTro = v.IDVaiTro AND q.MaModule = m.MaModule
  );

-- Seed cho các vai trò khác có QuyenHan > 0 (khác 15):
-- cấp quyền theo bitmask cho tất cả module
INSERT INTO VaiTroQuyen (IDVaiTro, MaModule, CoQuyenXem, CoQuyenThem, CoQuyenSua, CoQuyenXoa)
SELECT v.IDVaiTro, m.MaModule,
       CASE WHEN (v.QuyenHan & 1) = 1 THEN 1 ELSE 0 END,
       CASE WHEN (v.QuyenHan & 2) = 2 THEN 1 ELSE 0 END,
       CASE WHEN (v.QuyenHan & 4) = 4 THEN 1 ELSE 0 END,
       CASE WHEN (v.QuyenHan & 8) = 8 THEN 1 ELSE 0 END
FROM VaiTro v
CROSS JOIN (
    SELECT 'dashboard' AS MaModule UNION ALL
    SELECT 'trangchu' UNION ALL
    SELECT 'thongtinungho' UNION ALL
    SELECT 'danhsachungho' UNION ALL
    SELECT 'soduquy' UNION ALL
    SELECT 'ketquachamlo' UNION ALL
    SELECT 'diadiembando' UNION ALL
    SELECT 'nguoidancantrogium' UNION ALL
    SELECT 'gopy' UNION ALL
    SELECT 'banner' UNION ALL
    SELECT 'cauhinh' UNION ALL
    SELECT 'quanlynguoidung'
) m
WHERE v.QuyenHan IS NOT NULL
  AND v.QuyenHan > 0
  AND v.QuyenHan <> 15
  AND NOT EXISTS (
      SELECT 1 FROM VaiTroQuyen q 
      WHERE q.IDVaiTro = v.IDVaiTro AND q.MaModule = m.MaModule
  );
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Xóa tất cả bản ghi VaiTroQuyen do migration này seed
            migrationBuilder.Sql(@"
DELETE FROM VaiTroQuyen
WHERE IDVaiTro IN (
    SELECT IDVaiTro FROM VaiTro
);
");
        }
    }
}