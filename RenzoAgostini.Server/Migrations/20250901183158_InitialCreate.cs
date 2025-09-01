using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RenzoAgostini.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Paintings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Year = table.Column<int>(type: "INTEGER", nullable: true),
                    Medium = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    IsForSale = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paintings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaintingImages",
                columns: table => new
                {
                    Url = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    PaintingId = table.Column<int>(type: "INTEGER", nullable: false),
                    Width = table.Column<int>(type: "INTEGER", nullable: true),
                    Height = table.Column<int>(type: "INTEGER", nullable: true),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaintingImages", x => new { x.PaintingId, x.Url });
                    table.ForeignKey(
                        name: "FK_PaintingImages_Paintings_PaintingId",
                        column: x => x.PaintingId,
                        principalTable: "Paintings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Paintings",
                columns: new[] { "Id", "Description", "IsForSale", "Medium", "Price", "Slug", "Title", "Year" },
                values: new object[,]
                {
                    { 1, "Serie marina. Colori caldi.", true, "Olio su tela", 1200m, "alba-sul-mare", "Alba sul mare", 2023 },
                    { 2, "Acrilico, toni blu.", false, "Acrilico su tavola", null, "notturno", "Notturno", 2021 },
                    { 3, "Paesaggio primaverile.", true, "Olio su tela", 900m, "colline", "Colline", 2024 }
                });

            migrationBuilder.InsertData(
                table: "PaintingImages",
                columns: new[] { "PaintingId", "Url", "Height", "IsPrimary", "Width" },
                values: new object[,]
                {
                    { 1, "/img/q1a.jpg", 600, true, 800 },
                    { 1, "/img/q1b.jpg", 600, false, 800 },
                    { 2, "/img/q2a.jpg", 600, true, 800 },
                    { 2, "/img/q2b.jpg", 600, false, 800 },
                    { 3, "/img/q3a.jpg", 600, true, 800 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Paintings_Slug",
                table: "Paintings",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaintingImages");

            migrationBuilder.DropTable(
                name: "Paintings");
        }
    }
}
