using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OgrenciAidatSistemi.Migrations
{
    /// <inheritdoc />
    public partial class init_mgrt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SiteAdminId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Schools",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Schools");

            migrationBuilder.AddColumn<int>(
                name: "SiteAdminId",
                table: "Users",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
