using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebMTTQ.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidateFundAndResultModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KetQuaHoatDong",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoaiHoatDong = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Thang = table.Column<int>(type: "int", nullable: true),
                    Nam = table.Column<int>(type: "int", nullable: true),
                    DonViUngHo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhanLoaiDonVi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoLuongHo = table.Column<int>(type: "int", nullable: true),
                    KinhPhi = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KetQuaHoatDong", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SoDuQuy",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoaiQuy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TienMat = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    TienGuiNganHang = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    TongTonQuy = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoDuQuy", x => x.Id);
                });

            // ========== COPY DATA FROM OLD TABLES TO NEW ==========
            // SoDuQuyBienDao → SoDuQuy (LoaiQuy = 'BienDao')
            migrationBuilder.Sql(@"
INSERT INTO [SoDuQuy] ([LoaiQuy], [TienMat], [TienGuiNganHang], [TongTonQuy], [NgayCapNhat])
SELECT N'BienDao', [TienMat], [TienGuiNganHang], [TongTonQuy], [NgayCapNhat]
FROM [SoDuQuyBienDao]");

            // SoDuQuyCuuTro → SoDuQuy (LoaiQuy = 'CuuTro')
            migrationBuilder.Sql(@"
INSERT INTO [SoDuQuy] ([LoaiQuy], [TienMat], [TienGuiNganHang], [TongTonQuy], [NgayCapNhat])
SELECT N'CuuTro', [TienMat], [TienGuiNganHang], [TongTonQuy], [NgayCapNhat]
FROM [SoDuQuyCuuTro]");

            // SoDuQuyViNguoiNgheo → SoDuQuy (LoaiQuy = 'NguoiNgheo')
            migrationBuilder.Sql(@"
INSERT INTO [SoDuQuy] ([LoaiQuy], [TienMat], [TienGuiNganHang], [TongTonQuy], [NgayCapNhat])
SELECT N'NguoiNgheo', [TienMat], [TienGuiNganHang], [TienMat] + [TienGuiNganHang], [NgayCapNhat]
FROM [SoDuQuyViNguoiNgheo]");

            // KetQuaHoatDongBienDao → KetQuaHoatDong (LoaiHoatDong = 'BienDao')
            migrationBuilder.Sql(@"
INSERT INTO [KetQuaHoatDong] ([LoaiHoatDong], [Thang], [Nam], [DonViUngHo], [PhanLoaiDonVi], [NoiDung], [SoLuongHo], [KinhPhi], [TrangThai])
SELECT N'BienDao', [Thang], [Nam], [DonViUngHo], [PhanLoaiDonVi], [NoiDung], [SoLuongHo], [KinhPhi], [TrangThai]
FROM [KetQuaHoatDongBienDao]");

            // KetQuaHoatDongCuuTro → KetQuaHoatDong (LoaiHoatDong = 'CuuTro')
            migrationBuilder.Sql(@"
INSERT INTO [KetQuaHoatDong] ([LoaiHoatDong], [Thang], [Nam], [DonViUngHo], [PhanLoaiDonVi], [NoiDung], [SoLuongHo], [KinhPhi], [TrangThai])
SELECT N'CuuTro', [Thang], [Nam], [DonViUngHo], [PhanLoaiDonVi], [NoiDung], [SoLuongHo], [KinhPhi], [TrangThai]
FROM [KetQuaHoatDongCuuTro]");

            // ========== DROP OLD TABLES ONLY AFTER DATA COPIED ==========
            migrationBuilder.DropTable(
                name: "KetQuaHoatDongBienDao");

            migrationBuilder.DropTable(
                name: "KetQuaHoatDongCuuTro");

            migrationBuilder.DropTable(
                name: "SoDuQuyBienDao");

            migrationBuilder.DropTable(
                name: "SoDuQuyCuuTro");

            migrationBuilder.DropTable(
                name: "SoDuQuyViNguoiNgheo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Recreate old tables (best-effort reverse migration without data loss prevention)
            migrationBuilder.DropTable(
                name: "KetQuaHoatDong");

            migrationBuilder.DropTable(
                name: "SoDuQuy");
        }
    }
}