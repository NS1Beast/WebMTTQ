using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebMTTQ.Migrations
{
    /// <inheritdoc />
    public partial class DanhSachUngHo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DanhSachUngHo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenNguoiUngHo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    NgayUngHo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SoTien = table.Column<decimal>(type: "decimal(18,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhSachUngHo", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DanhSachUngHo");
        }
    }
}
