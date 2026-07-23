using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebMTTQ.Migrations
{
    /// <inheritdoc />
    public partial class AddNgayThucHienToDiaDiemBanDo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NgayThucHien",
                table: "DiaDiemBanDo",
                type: "datetime2",
                nullable: true);

         
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
           

            migrationBuilder.DropColumn(
                name: "NgayThucHien",
                table: "DiaDiemBanDo");
        }
    }
}
