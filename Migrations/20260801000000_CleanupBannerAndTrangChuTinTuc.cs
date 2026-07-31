using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebMTTQ.Migrations
{
    /// <inheritdoc />
    public partial class CleanupBannerAndTrangChuTinTuc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop duplicate Banners table (plural) if it still exists in the database
            migrationBuilder.Sql(@"IF EXISTS (SELECT * FROM sys.tables WHERE name='Banners') DROP TABLE Banners;");

            // Ensure TrangChuTinTuc table exists (in case previous migration failed before creating it)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='TrangChuTinTuc')
                CREATE TABLE TrangChuTinTuc (
                    Id int IDENTITY(1,1) PRIMARY KEY,
                    IdTrangChuMuc int NOT NULL,
                    TieuDe nvarchar(500) NOT NULL DEFAULT '',
                    TomTat nvarchar(max) NULL,
                    HinhAnh nvarchar(1000) NULL,
                    LienKet nvarchar(1000) NULL,
                    ThuTu int NOT NULL DEFAULT 0,
                    TrangThai bit NOT NULL DEFAULT 1,
                    NgayTao datetime2 NOT NULL DEFAULT GETDATE(),
                    CONSTRAINT FK_TrangChuTinTuc_TrangChuMuc FOREIGN KEY (IdTrangChuMuc) REFERENCES TrangChuMuc(Id) ON DELETE CASCADE
                );
            ");

            // Ensure index on IdTrangChuMuc for faster queries
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_TrangChuTinTuc_IdTrangChuMuc')
                CREATE INDEX IX_TrangChuTinTuc_IdTrangChuMuc ON TrangChuTinTuc(IdTrangChuMuc);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No action needed for downgrade
        }
    }
}
