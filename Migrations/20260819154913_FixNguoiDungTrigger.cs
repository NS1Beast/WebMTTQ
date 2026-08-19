using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebMTTQ.Migrations
{
    /// <inheritdoc />
    public partial class FixNguoiDungTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Batch 1: Drop trigger cũ (trigger này vẫn tham chiếu cột DaXoa đã bị xóa)
            // => gây lỗi "Invalid column name 'DaXoa'" khi UPDATE bảng NguoiDung.
            migrationBuilder.Sql(@"
IF EXISTS (SELECT * FROM sys.triggers WHERE name = N'trg_NguoiDung_UpdateNgay' AND parent_id = OBJECT_ID(N'[dbo].[NguoiDung]'))
BEGIN
    DROP TRIGGER [dbo].[trg_NguoiDung_UpdateNgay];
END
");

            // Batch 2: Tạo trigger mới sạch — chỉ dùng các cột hiện có, KHÔNG dùng DaXoa.
            // (CREATE TRIGGER phải là câu lệnh đầu tiên trong batch, nên tách riêng)
            migrationBuilder.Sql(@"
CREATE TRIGGER [dbo].[trg_NguoiDung_UpdateNgay]
ON [dbo].[NguoiDung]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[NguoiDung]
    SET [NgayCapNhat] = GETDATE()
    FROM [dbo].[NguoiDung] nd
    INNER JOIN inserted i ON nd.[IDNguoiDung] = i.[IDNguoiDung];
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore trigger cũ (nếu cần rollback)
            migrationBuilder.Sql(@"
IF EXISTS (SELECT * FROM sys.triggers WHERE name = N'trg_NguoiDung_UpdateNgay' AND parent_id = OBJECT_ID(N'[dbo].[NguoiDung]'))
BEGIN
    DROP TRIGGER [dbo].[trg_NguoiDung_UpdateNgay];
END
");
        }
    }
}