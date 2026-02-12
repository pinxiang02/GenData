using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataGen_v1.Migrations
{
    /// <inheritdoc />
    public partial class AddGeneratorConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsApiEnabled",
                table: "Generators",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDbEnabled",
                table: "Generators",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApiEnabled",
                table: "Generators");

            migrationBuilder.DropColumn(
                name: "IsDbEnabled",
                table: "Generators");
        }
    }
}
