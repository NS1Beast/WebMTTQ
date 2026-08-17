using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebMTTQ.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBannerTitleDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop TieuDe column from Banner table
            migrationBuilder.DropColumn(
                name: "TieuDe",
                table: "Banner");

            // Drop MoTa column from Banner table
            migrationBuilder.DropColumn(
                name: "MoTa",
                table: "Banner");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Re-add TieuDe column
            migrationBuilder.AddColumn<string>(
                name: "TieuDe",
                table: "Banner",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            // Re-add MoTa column
            migrationBuilder.AddColumn<string>(
                name: "MoTa",
                table: "Banner",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }
    }
}