using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebMTTQ.Migrations
{
    /// <inheritdoc />
    public partial class AddTrangChuMucAndBannerFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create Banner table if not exists (with new fields for advanced slider)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='Banner')
                CREATE TABLE Banner (
                    IdBanner int IDENTITY(1,1) PRIMARY KEY,
                    TieuDe nvarchar(500) NULL,
                    HinhAnh nvarchar(1000) NULL,
                    LienKet nvarchar(1000) NULL,
                    ThuTu int NOT NULL DEFAULT 0,
                    TrangThai bit NOT NULL DEFAULT 1,
                    MoTa nvarchar(1000) NULL,
                    HieuUng nvarchar(50) NULL DEFAULT 'slide',
                    TocDo int NOT NULL DEFAULT 600,
                    ThoiGianDung int NOT NULL DEFAULT 5000,
                    MauNen nvarchar(50) NULL DEFAULT '#1a1a2e'
                );
            ");

            // Create TrangChuMuc table if not exists
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='TrangChuMuc')
                CREATE TABLE TrangChuMuc (
                    Id int IDENTITY(1,1) PRIMARY KEY,
                    TieuDe nvarchar(500) NOT NULL DEFAULT '',
                    Loai nvarchar(100) NOT NULL DEFAULT 'tin-tuc',
                    NoiDung nvarchar(max) NULL,
                    HinhAnh nvarchar(500) NULL,
                    TrangThai bit NOT NULL DEFAULT 1,
                    ThuTu int NOT NULL DEFAULT 0,
                    NgayTao datetime2 NOT NULL DEFAULT GETDATE(),
                    NgayCapNhat datetime2 NULL
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"IF EXISTS (SELECT * FROM sys.tables WHERE name='TrangChuMuc') DROP TABLE TrangChuMuc;");
            migrationBuilder.Sql(@"IF EXISTS (SELECT * FROM sys.tables WHERE name='Banner') DROP TABLE Banner;");
        }
    }
}