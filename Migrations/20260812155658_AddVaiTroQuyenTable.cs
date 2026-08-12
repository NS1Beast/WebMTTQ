using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebMTTQ.Migrations
{
    /// <inheritdoc />
    public partial class AddVaiTroQuyenTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VaiTroQuyen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IDVaiTro = table.Column<int>(type: "int", nullable: false),
                    MaModule = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CoQuyenXem = table.Column<bool>(type: "bit", nullable: false),
                    CoQuyenThem = table.Column<bool>(type: "bit", nullable: false),
                    CoQuyenSua = table.Column<bool>(type: "bit", nullable: false),
                    CoQuyenXoa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__VaiTroQuyen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VaiTroQuyen_VaiTro",
                        column: x => x.IDVaiTro,
                        principalTable: "VaiTro",
                        principalColumn: "IDVaiTro",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_VaiTroQuyen_VaiTro_Module",
                table: "VaiTroQuyen",
                columns: new[] { "IDVaiTro", "MaModule" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VaiTroQuyen");
        }
    }
}
