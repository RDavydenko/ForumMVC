using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MyForum.Migrations
{
    public partial class silencemod : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSilenced",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: false,
                defaultValueSql: "false");
            migrationBuilder.AddColumn<DateTime>(
                name: "SilenceStartTime",
                table: "AspNetUsers",
                nullable: true
                );
            migrationBuilder.AddColumn<DateTime>(
                name: "SilenceStopTime",
                table: "AspNetUsers",
                nullable: true
                );           
        }

           

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("IsSilenced", "AspNetUsers");  
            migrationBuilder.DropColumn("SilenceStartTime", "AspNetUsers");  
            migrationBuilder.DropColumn("SilenceStopTime", "AspNetUsers");  
        }
    }
}
