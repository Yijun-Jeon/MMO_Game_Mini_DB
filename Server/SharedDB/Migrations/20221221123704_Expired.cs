using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SharedDB.Migrations
{
    public partial class Expired : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Created",
                table: "Token");

            migrationBuilder.AddColumn<DateTime>(
                name: "Expired",
                table: "Token",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Expired",
                table: "Token");

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Token",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
