using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MyForum.Migrations
{
    public partial class isHiddenandisClosedinthemes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {            
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatingTime",
                table: "Themes",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatingTime",
                table: "Messages",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<bool>(
                name: "IsClosed",
                table: "Themes",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsHidden",
                table: "Themes",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsHidden",
                table: "Sections",
                nullable: false,
                defaultValue: false);            

            migrationBuilder.AddColumn<bool>(
                name: "IsHidden",
                table: "Categories",
                nullable: false,
                defaultValue: false);          
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        { 
            migrationBuilder.DropColumn(
                name: "IsClosed",
                table: "Themes");

            migrationBuilder.DropColumn(
                name: "IsHidden",
                table: "Themes");

            migrationBuilder.DropColumn(
                name: "IsHidden",
                table: "Sections");

            migrationBuilder.DropColumn(
                name: "IsHidden",
                table: "Categories");           
        }
    }
}
