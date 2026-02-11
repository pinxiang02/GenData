using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DataGen_v1.Migrations
{
    /// <inheritdoc />
    public partial class AddApiConfig2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApiConfigId",
                table: "Generators",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ApiConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProfileName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TargetUrl = table.Column<string>(type: "text", nullable: false),
                    Method = table.Column<string>(type: "text", nullable: false),
                    HeaderName = table.Column<string>(type: "text", nullable: true),
                    HeaderValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiConfigs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Generators_ApiConfigId",
                table: "Generators",
                column: "ApiConfigId");

            migrationBuilder.AddForeignKey(
                name: "FK_Generators_ApiConfigs_ApiConfigId",
                table: "Generators",
                column: "ApiConfigId",
                principalTable: "ApiConfigs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Generators_ApiConfigs_ApiConfigId",
                table: "Generators");

            migrationBuilder.DropTable(
                name: "ApiConfigs");

            migrationBuilder.DropIndex(
                name: "IX_Generators_ApiConfigId",
                table: "Generators");

            migrationBuilder.DropColumn(
                name: "ApiConfigId",
                table: "Generators");
        }
    }
}
