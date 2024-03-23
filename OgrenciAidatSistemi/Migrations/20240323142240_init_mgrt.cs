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
        }
    }
}
