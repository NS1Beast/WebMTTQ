using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebMTTQ.Migrations
{
    /// <inheritdoc />
    public partial class ThongTinUngHo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           

         

            migrationBuilder.CreateTable(
                name: "ThongTinNhanUngHos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenNgânHang = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChiNhanh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QrCodeUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenTaiKhoan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoTaiKhoan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DonViThuHuong = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoiDungChuyenKhoan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongTinNhanUngHos", x => x.Id);
                });


          
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
          
            migrationBuilder.DropTable(
                name: "ThongTinNhanUngHos");

        }
    }
}
