using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RenzoAgostini.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddBiography : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Content = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bios", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Bios",
                columns: new[] { "Id", "Content", "ImageUrl" },
                values: new object[] { 1, "Questa è la biografia dell'artista. Modificala dal pannello admin.", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bios");
        }
    }
}
