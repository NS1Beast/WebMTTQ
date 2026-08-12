using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebMTTQ.Migrations
{
    /// <inheritdoc />
    public partial class RefactorQuyenVaoVaiTro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Xóa toàn bộ dữ liệu người dùng trước (do FK từ QuyenTruyCap, NhatKyHeThong, etc.)
            migrationBuilder.Sql(@"
                DELETE FROM [NhatKyHeThong];
                DELETE FROM [BaiViet];
                DELETE FROM [KhoanDongGop];
                DELETE FROM [LuotTraoTang];
                DELETE FROM [DonXinHoTro];
                DELETE FROM [HopThuGopY];
                DELETE FROM [NguoiDung];
            ");

            // 2. Xóa bảng QuyenTruyCap (sau khi đã xóa NguoiDung)
            migrationBuilder.DropTable(
                name: "QuyenTruyCap");

            // 3. Thêm cột NgayTao, NgayCapNhat vào bảng VaiTro
            migrationBuilder.AddColumn<DateTime>(
                name: "NgayCapNhat",
                table: "VaiTro",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())");

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayTao",
                table: "VaiTro",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())");

            // 4. Tạo vai trò Admin mặc định với toàn quyền (JSON trong QuyenHan)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [VaiTro] WHERE [TenVaiTro] = N'Quản trị viên')
                BEGIN
                    DECLARE @AdminJson NVARCHAR(MAX) = N'[
                        {""MaModule"":""dashboard"",""TenModule"":""Dashboard"",""CoQuyenXem"":1,""CoQuyenThem"":1,""CoQuyenSua"":1,""CoQuyenXoa"":1},
                        {""MaModule"":""trangchu"",""TenModule"":""Cài đặt Trang chủ"",""CoQuyenXem"":1,""CoQuyenThem"":1,""CoQuyenSua"":1,""CoQuyenXoa"":1},
                        {""MaModule"":""thongtinungho"",""TenModule"":""Thông tin ủng hộ"",""CoQuyenXem"":1,""CoQuyenThem"":1,""CoQuyenSua"":1,""CoQuyenXoa"":1},
                        {""MaModule"":""danhsachungho"",""TenModule"":""Danh sách ủng hộ"",""CoQuyenXem"":1,""CoQuyenThem"":1,""CoQuyenSua"":1,""CoQuyenXoa"":1},
                        {""MaModule"":""soduquy"",""TenModule"":""Số dư Quỹ"",""CoQuyenXem"":1,""CoQuyenThem"":1,""CoQuyenSua"":1,""CoQuyenXoa"":1},
                        {""MaModule"":""ketquachamlo"",""TenModule"":""Kết quả chăm lo"",""CoQuyenXem"":1,""CoQuyenThem"":1,""CoQuyenSua"":1,""CoQuyenXoa"":1},
                        {""MaModule"":""diadiembando"",""TenModule"":""Bản đồ an sinh"",""CoQuyenXem"":1,""CoQuyenThem"":1,""CoQuyenSua"":1,""CoQuyenXoa"":1},
                        {""MaModule"":""nguoidancantrogium"",""TenModule"":""Yêu cầu trợ giúp"",""CoQuyenXem"":1,""CoQuyenThem"":1,""CoQuyenSua"":1,""CoQuyenXoa"":1},
                        {""MaModule"":""gopy"",""TenModule"":""Hộp thư góp ý"",""CoQuyenXem"":1,""CoQuyenThem"":1,""CoQuyenSua"":1,""CoQuyenXoa"":1},
                        {""MaModule"":""banner"",""TenModule"":""Quản lý Banner"",""CoQuyenXem"":1,""CoQuyenThem"":1,""CoQuyenSua"":1,""CoQuyenXoa"":1},
                        {""MaModule"":""cauhinh"",""TenModule"":""Cài đặt hệ thống"",""CoQuyenXem"":1,""CoQuyenThem"":1,""CoQuyenSua"":1,""CoQuyenXoa"":1},
                        {""MaModule"":""quanlynguoidung"",""TenModule"":""Quản lý người dùng"",""CoQuyenXem"":1,""CoQuyenThem"":1,""CoQuyenSua"":1,""CoQuyenXoa"":1}
                    ]';

                    INSERT INTO [VaiTro] ([TenVaiTro], [QuyenHan], [DaXoa])
                    VALUES (N'Quản trị viên', @AdminJson, 0);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NgayCapNhat",
                table: "VaiTro");

            migrationBuilder.DropColumn(
                name: "NgayTao",
                table: "VaiTro");

            migrationBuilder.CreateTable(
                name: "QuyenTruyCap",
                columns: table => new
                {
                    IDQuyen = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IDNguoiDung = table.Column<int>(type: "int", nullable: false),
                    CoQuyenSua = table.Column<bool>(type: "bit", nullable: false),
                    CoQuyenThem = table.Column<bool>(type: "bit", nullable: false),
                    CoQuyenXem = table.Column<bool>(type: "bit", nullable: false),
                    CoQuyenXoa = table.Column<bool>(type: "bit", nullable: false),
                    MaModule = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    NgayTao = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    TenModule = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
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
    }
}