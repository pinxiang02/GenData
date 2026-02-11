using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataGen_v1.Migrations
{
    /// <inheritdoc />
    public partial class AddParentToNodeConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "NodeConfigs",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NodeConfigs_ParentId",
                table: "NodeConfigs",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_NodeConfigs_NodeConfigs_ParentId",
                table: "NodeConfigs",
                column: "ParentId",
                principalTable: "NodeConfigs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NodeConfigs_NodeConfigs_ParentId",
                table: "NodeConfigs");

            migrationBuilder.DropIndex(
                name: "IX_NodeConfigs_ParentId",
                table: "NodeConfigs");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "NodeConfigs");
        }
    }
}
