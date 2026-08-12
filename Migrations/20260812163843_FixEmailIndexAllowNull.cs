using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebMTTQ.Migrations
{
    /// <inheritdoc />
    public partial class FixEmailIndexAllowNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Sửa lỗi: Email nullable nhưng có UNIQUE constraint
            // SQL Server coi NULL = NULL, nên khi tạo nhiều người dùng không có email (NULL)
            // sẽ vi phạm UNIQUE KEY constraint.
            //
            // Giải pháp: drop UNIQUE constraint cũ và thay bằng filtered unique index
            // chỉ áp dụng cho các dòng có Email IS NOT NULL.

            // 1. Xóa unique constraint/index cũ (nếu có)
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.indexes 
                           WHERE name = 'UQ__NguoiDun__A9D105348E5266DA' 
                           AND object_id = OBJECT_ID('dbo.NguoiDung'))
                BEGIN
                    -- Kiểm tra xem nó là constraint hay index
                    IF EXISTS (SELECT 1 FROM sys.key_constraints 
                               WHERE name = 'UQ__NguoiDun__A9D105348E5266DA' 
                               AND parent_object_id = OBJECT_ID('dbo.NguoiDung'))
                    BEGIN
                        ALTER TABLE [dbo].[NguoiDung] 
                        DROP CONSTRAINT [UQ__NguoiDun__A9D105348E5266DA];
                    END
                    ELSE
                    BEGIN
                        DROP INDEX [UQ__NguoiDun__A9D105348E5266DA] ON [dbo].[NguoiDung];
                    END
                END
            ");

            // 2. Đồng bộ dữ liệu hiện có - nếu có nhiều user có cùng email (không NULL),
            //    giữ user đầu tiên, set email = NULL cho các user trùng lặp
            migrationBuilder.Sql(@"
                -- Tìm các email trùng lặp và set NULL cho các bản sao (giữ bản đầu tiên)
                UPDATE nd
                SET nd.Email = NULL
                FROM [dbo].[NguoiDung] nd
                INNER JOIN (
                    SELECT [Email], MIN([IDNguoiDung]) AS MinId
                    FROM [dbo].[NguoiDung]
                    WHERE [Email] IS NOT NULL
                    GROUP BY [Email]
                    HAVING COUNT(*) > 1
                ) dup ON nd.Email = dup.Email AND nd.[IDNguoiDung] > dup.MinId;
            ");

            // 3. Tạo filtered unique index mới - chỉ áp dụng khi Email IS NOT NULL
            //    Cho phép nhiều người dùng có Email = NULL (không vi phạm UNIQUE)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes 
                               WHERE name = 'UQ__NguoiDun__A9D105348E5266DA' 
                               AND object_id = OBJECT_ID('dbo.NguoiDung'))
                BEGIN
                    CREATE UNIQUE NONCLUSTERED INDEX [UQ__NguoiDun__A9D105348E5266DA]
                    ON [dbo].[NguoiDung]([Email])
                    WHERE [Email] IS NOT NULL;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback: drop filtered index và tạo lại unique constraint cũ
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.indexes 
                           WHERE name = 'UQ__NguoiDun__A9D105348E5266DA' 
                           AND object_id = OBJECT_ID('dbo.NguoiDung'))
                BEGIN
                    DROP INDEX [UQ__NguoiDun__A9D105348E5266DA] ON [dbo].[NguoiDung];
                END

                ALTER TABLE [dbo].[NguoiDung] 
                ADD CONSTRAINT [UQ__NguoiDun__A9D105348E5266DA] UNIQUE NONCLUSTERED ([Email]);
            ");
        }
    }
}