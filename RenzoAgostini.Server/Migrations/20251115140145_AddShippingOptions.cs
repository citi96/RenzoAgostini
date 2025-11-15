using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RenzoAgostini.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddShippingOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ItemsTotal",
                table: "Orders",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingCost",
                table: "Orders",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ShippingEstimatedDelivery",
                table: "Orders",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingFreeThreshold",
                table: "Orders",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShippingIsPickup",
                table: "Orders",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ShippingMethodName",
                table: "Orders",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ShippingOptionId",
                table: "Orders",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ShippingOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Cost = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    FreeShippingThreshold = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    SupportsItaly = table.Column<bool>(type: "INTEGER", nullable: false),
                    SupportsInternational = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsPickup = table.Column<bool>(type: "INTEGER", nullable: false),
                    EstimatedDelivery = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingOptions", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ShippingOptions",
                columns: new[] { "Id", "Cost", "CreatedAt", "Description", "EstimatedDelivery", "FreeShippingThreshold", "IsActive", "IsPickup", "Name", "SupportsInternational", "SupportsItaly", "UpdatedAt" },
                values: new object[] { 1, 0m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ritira direttamente presso lo studio dell'artista.", "Su appuntamento", null, true, true, "Ritiro a mano", false, true, null });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ShippingOptionId",
                table: "Orders",
                column: "ShippingOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingOptions_Name",
                table: "ShippingOptions",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_ShippingOptions_ShippingOptionId",
                table: "Orders",
                column: "ShippingOptionId",
                principalTable: "ShippingOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_ShippingOptions_ShippingOptionId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "ShippingOptions");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ShippingOptionId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ItemsTotal",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingCost",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingEstimatedDelivery",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingFreeThreshold",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingIsPickup",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingMethodName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingOptionId",
                table: "Orders");
        }
    }
}
