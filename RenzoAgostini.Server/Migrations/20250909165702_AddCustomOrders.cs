using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RenzoAgostini.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CustomerEmail = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    AttachmentPath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    AttachmentOriginalName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    QuotedPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    ArtistNotes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    PaintingId = table.Column<int>(type: "INTEGER", nullable: true),
                    AccessCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomOrders_Paintings_PaintingId",
                        column: x => x.PaintingId,
                        principalTable: "Paintings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomOrders_AccessCode",
                table: "CustomOrders",
                column: "AccessCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomOrders_PaintingId",
                table: "CustomOrders",
                column: "PaintingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomOrders");
        }
    }
}
