using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebMTTQ.Migrations
{
    /// <inheritdoc />
    public partial class AddTrangChuTinTucAndDropOldBanners : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop any leftover duplicate Banners table (plural) if it exists in the database
            migrationBuilder.Sql(@"IF EXISTS (SELECT * FROM sys.tables WHERE name='Banners') DROP TABLE Banners;");

            migrationBuilder.CreateTable(
                name: "TrangChuTinTuc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdTrangChuMuc = table.Column<int>(type: "int", nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TomTat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HinhAnh = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    LienKet = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ThuTu = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrangChuTinTuc", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrangChuTinTuc_TrangChuMuc_IdTrangChuMuc",
                        column: x => x.IdTrangChuMuc,
                        principalTable: "TrangChuMuc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrangChuTinTuc_IdTrangChuMuc",
                table: "TrangChuTinTuc",
                column: "IdTrangChuMuc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrangChuTinTuc");
        }
    }
}
