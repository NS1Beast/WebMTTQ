using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebMTTQ.Migrations
{
    /// <inheritdoc />
    public partial class ChangeQuyenHanToTinyInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Chuyển đổi dữ liệu quyền hiện tại từ JSON string sang bitmask tinyint
            //    - Vai trò Admin (Quản trị viên) hiện có toàn quyền -> set = 15 (1+2+4+8)
            //    - Các vai trò khác có JSON chứa quyền -> tính bitmask từ JSON
            //    - Vai trò không có quyền -> set = 0
            migrationBuilder.Sql(@"
                -- Chuyển QuyenHan từ JSON string sang bitmask (tinyint)
                UPDATE [VaiTro]
                SET [QuyenHan] = 
                    CASE 
                        WHEN [TenVaiTro] LIKE N'%Quản trị%' OR [TenVaiTro] LIKE N'%Quan tri%' OR [TenVaiTro] LIKE N'%Admin%'
                            THEN 15
                        WHEN [QuyenHan] IS NULL OR [QuyenHan] = '' OR [QuyenHan] = N'' 
                            THEN 0
                        -- Nếu có bất kỳ quyền nào trong JSON thì là 15 (toàn quyền tất cả modules)
                        WHEN CHARINDEX(N'CoQuyenXem', [QuyenHan]) > 0 
                            THEN 15
                        ELSE 0
                    END;
            ");

            // 2. Đổi kiểu cột từ nvarchar(max) sang tinyint
            migrationBuilder.AlterColumn<byte>(
                name: "QuyenHan",
                table: "VaiTro",
                type: "tinyint",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "QuyenHan",
                table: "VaiTro",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint",
                oldNullable: true);
        }
    }
}
