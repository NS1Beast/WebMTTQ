using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebMTTQ.Migrations
{
    /// <inheritdoc />
    public partial class AddQuyenTruyCapAndMaXacThuc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MaXacThuc",
                columns: table => new
                {
                    IDMaXacThuc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    MaOtp = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    HanHet = table.Column<DateTime>(type: "datetime", nullable: false),
                    DaSuDung = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DiaChiIp = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__MaXacThuc", x => x.IDMaXacThuc);
                });

            migrationBuilder.CreateTable(
                name: "QuyenTruyCap",
                columns: table => new
                {
                    IDQuyen = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IDNguoiDung = table.Column<int>(type: "int", nullable: false),
                    MaModule = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    TenModule = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CoQuyenXem = table.Column<bool>(type: "bit", nullable: false),
                    CoQuyenThem = table.Column<bool>(type: "bit", nullable: false),
                    CoQuyenSua = table.Column<bool>(type: "bit", nullable: false),
                    CoQuyenXoa = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__QuyenTruyCap", x => x.IDQuyen);
                    table.ForeignKey(
                        name: "FK_QuyenTruyCap_NguoiDung",
                        column: x => x.IDNguoiDung,
                        principalTable: "NguoiDung",
                        principalColumn: "IDNguoiDung",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuyenTruyCap_IDNguoiDung",
                table: "QuyenTruyCap",
                column: "IDNguoiDung");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaXacThuc");

            migrationBuilder.DropTable(
                name: "QuyenTruyCap");
        }
    }
}
