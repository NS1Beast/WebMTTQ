using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebMTTQ.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSoftDeleteColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IDX_VanBan_DaXoa",
                table: "VanBanTaiLieu");

            migrationBuilder.DropIndex(
                name: "IDX_LuotTraoTang_DaXoa",
                table: "LuotTraoTang");

            migrationBuilder.DropIndex(
                name: "IDX_KhoanDongGop_DaXoa",
                table: "KhoanDongGop");

            migrationBuilder.DropIndex(
                name: "IDX_DiaDiemBanDo_DaXoa",
                table: "DiaDiemBanDo");

            migrationBuilder.DropIndex(
                name: "IDX_BaiViet_DaXoa",
                table: "BaiViet");

            migrationBuilder.DropColumn(
                name: "DaXoa",
                table: "VanBanTaiLieu");

            migrationBuilder.DropColumn(
                name: "DaXoa",
                table: "VaiTro");

            migrationBuilder.DropColumn(
                name: "DaXoa",
                table: "ThanhPhanGiaoDien");

            migrationBuilder.DropColumn(
                name: "DaXoa",
                table: "NhaHaoTam");

            migrationBuilder.DropColumn(
                name: "DaXoa",
                table: "NguoiDung");

            migrationBuilder.DropColumn(
                name: "DaXoa",
                table: "NguoiDanCanTroGiup");

            migrationBuilder.DropColumn(
                name: "DaXoa",
                table: "NguoiCanGiupDo");

            migrationBuilder.DropColumn(
                name: "DaXoa",
                table: "LuotTraoTang");

            migrationBuilder.DropColumn(
                name: "DaXoa",
                table: "KhoanDongGop");

            migrationBuilder.DropColumn(
                name: "DaXoa",
                table: "HopThuGopY");

            migrationBuilder.DropColumn(
                name: "DaXoa",
                table: "DonXinHoTro");

            migrationBuilder.DropColumn(
                name: "DaXoa",
                table: "DoanTheToChuc");

            migrationBuilder.DropColumn(
                name: "DaXoa",
                table: "DiaDiemBanDo");

            migrationBuilder.DropColumn(
                name: "DaXoa",
                table: "DanhMucQuy");

            migrationBuilder.DropColumn(
                name: "DaXoa",
                table: "ChuyenMuc");

            migrationBuilder.DropColumn(
                name: "DaXoa",
                table: "ChuongTrinhHoTro");

            migrationBuilder.DropColumn(
                name: "DaXoa",
                table: "BaiViet");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DaXoa",
                table: "VanBanTaiLieu",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DaXoa",
                table: "VaiTro",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DaXoa",
                table: "ThanhPhanGiaoDien",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DaXoa",
                table: "NhaHaoTam",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DaXoa",
                table: "NguoiDung",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DaXoa",
                table: "NguoiDanCanTroGiup",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DaXoa",
                table: "NguoiCanGiupDo",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DaXoa",
                table: "LuotTraoTang",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DaXoa",
                table: "KhoanDongGop",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DaXoa",
                table: "HopThuGopY",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DaXoa",
                table: "DonXinHoTro",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DaXoa",
                table: "DoanTheToChuc",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DaXoa",
                table: "DiaDiemBanDo",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DaXoa",
                table: "DanhMucQuy",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DaXoa",
                table: "ChuyenMuc",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DaXoa",
                table: "ChuongTrinhHoTro",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DaXoa",
                table: "BaiViet",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IDX_VanBan_DaXoa",
                table: "VanBanTaiLieu",
                column: "DaXoa");

            migrationBuilder.CreateIndex(
                name: "IDX_LuotTraoTang_DaXoa",
                table: "LuotTraoTang",
                column: "DaXoa");

            migrationBuilder.CreateIndex(
                name: "IDX_KhoanDongGop_DaXoa",
                table: "KhoanDongGop",
                column: "DaXoa");

            migrationBuilder.CreateIndex(
                name: "IDX_DiaDiemBanDo_DaXoa",
                table: "DiaDiemBanDo",
                column: "DaXoa");

            migrationBuilder.CreateIndex(
                name: "IDX_BaiViet_DaXoa",
                table: "BaiViet",
                column: "DaXoa");
        }
    }
}