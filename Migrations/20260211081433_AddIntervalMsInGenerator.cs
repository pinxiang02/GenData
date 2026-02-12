using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataGen_v1.Migrations
{
    /// <inheritdoc />
    public partial class AddIntervalMsInGenerator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IntervalMs",
                table: "Generators",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IntervalMs",
                table: "Generators");
        }
    }
}
