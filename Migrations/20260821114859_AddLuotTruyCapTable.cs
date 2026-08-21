using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebMTTQ.Migrations
{
    /// <inheritdoc />
    public partial class AddLuotTruyCapTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LuotTruyCap",
                columns: table => new
                {
                    IDLuotTruyCap = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    DuongDan = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ThoiGianTruyCap = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    LanTruyCapCuoi = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__LuotTruyCap", x => x.IDLuotTruyCap);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LuotTruyCap_LanTruyCapCuoi",
                table: "LuotTruyCap",
                column: "LanTruyCapCuoi");

            migrationBuilder.CreateIndex(
                name: "IX_LuotTruyCap_SessionId",
                table: "LuotTruyCap",
                column: "SessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LuotTruyCap");
        }
    }
}