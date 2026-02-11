using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataGen_v1.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGeneratorTable2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DbConnectionConfigId",
                table: "Generators",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TableName",
                table: "Generators",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Generators_DbConnectionConfigId",
                table: "Generators",
                column: "DbConnectionConfigId");

            migrationBuilder.AddForeignKey(
                name: "FK_Generators_DbConfigs_DbConnectionConfigId",
                table: "Generators",
                column: "DbConnectionConfigId",
                principalTable: "DbConfigs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Generators_DbConfigs_DbConnectionConfigId",
                table: "Generators");

            migrationBuilder.DropIndex(
                name: "IX_Generators_DbConnectionConfigId",
                table: "Generators");

            migrationBuilder.DropColumn(
                name: "DbConnectionConfigId",
                table: "Generators");

            migrationBuilder.DropColumn(
                name: "TableName",
                table: "Generators");
        }
    }
}
