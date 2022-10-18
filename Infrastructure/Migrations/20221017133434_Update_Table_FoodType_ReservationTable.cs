using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Update_Table_FoodType_ReservationTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Created",
                table: "ReservationTables");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ReservationTables");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ReservationTables");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ReservationTables");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "ReservationTables");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "ReservationTables");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "MenuFood");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "MenuFood");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "MenuFood");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "MenuFood");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "MenuFood");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "MenuFood");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "FoodType");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "FoodType");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "FoodType");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "FoodType");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "FoodType");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "FoodType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "ReservationTables",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ReservationTables",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "ReservationTables",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ReservationTables",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "ReservationTables",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "ReservationTables",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "MenuFood",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "MenuFood",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "MenuFood",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "MenuFood",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "MenuFood",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "MenuFood",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "FoodType",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "FoodType",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "FoodType",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "FoodType",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "FoodType",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "FoodType",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);
        }
    }
}
