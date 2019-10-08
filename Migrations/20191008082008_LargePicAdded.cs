using Microsoft.EntityFrameworkCore.Migrations;

namespace Manifest.Migrations
{
    public partial class LargePicAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FbProfilePicLargeUrl",
                table: "AspNetUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FbProfilePicLargeUrl",
                table: "AspNetUsers");
        }
    }
}
