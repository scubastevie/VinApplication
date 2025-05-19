using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinApplication.Migrations
{
    /// <inheritdoc />
    public partial class AddDealerVinModifiedDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColumnA",
                table: "Records");

            migrationBuilder.RenameColumn(
                name: "ColumnC",
                table: "Records",
                newName: "Vin");

            migrationBuilder.RenameColumn(
                name: "ColumnB",
                table: "Records",
                newName: "DealerId");

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "Records",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "Records");

            migrationBuilder.RenameColumn(
                name: "Vin",
                table: "Records",
                newName: "ColumnC");

            migrationBuilder.RenameColumn(
                name: "DealerId",
                table: "Records",
                newName: "ColumnB");

            migrationBuilder.AddColumn<string>(
                name: "ColumnA",
                table: "Records",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
