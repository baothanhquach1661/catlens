using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CatLens.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCatalogSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Variants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Handle = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Option1Value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Option2Value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Option3Value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Variants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryLevels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VariantId = table.Column<int>(type: "integer", nullable: false),
                    Location = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    OnHandQuantity = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryLevels_Variants_VariantId",
                        column: x => x.VariantId,
                        principalTable: "Variants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryLevels_VariantId_Location",
                table: "InventoryLevels",
                columns: new[] { "VariantId", "Location" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Variants_Handle_Option1Value_Option2Value_Option3Value",
                table: "Variants",
                columns: new[] { "Handle", "Option1Value", "Option2Value", "Option3Value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Variants_Sku",
                table: "Variants",
                column: "Sku");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryLevels");

            migrationBuilder.DropTable(
                name: "Variants");
        }
    }
}
