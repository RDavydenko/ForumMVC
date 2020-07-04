using Microsoft.EntityFrameworkCore.Migrations;

namespace MyForum.Migrations
{
    public partial class renamesectiondecsTriptionondescription : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Desctription",
                table: "Sections");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Sections",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Sections");

            migrationBuilder.AddColumn<string>(
                name: "Desctription",
                table: "Sections",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
