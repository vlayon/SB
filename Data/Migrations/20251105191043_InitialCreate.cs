using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pairs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstTokenAddress = table.Column<string>(type: "nvarchar(42)", maxLength: 42, nullable: false),
                    SecondTokenAddress = table.Column<string>(type: "nvarchar(42)", maxLength: 42, nullable: false),
                    PairAddress = table.Column<string>(type: "nvarchar(42)", maxLength: 42, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pairs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Trades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PairId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(38,18)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trades_Pairs_PairId",
                        column: x => x.PairId,
                        principalTable: "Pairs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pairs_FirstTokenAddress_SecondTokenAddress",
                table: "Pairs",
                columns: new[] { "FirstTokenAddress", "SecondTokenAddress" });

            migrationBuilder.CreateIndex(
                name: "IX_Pairs_PairAddress",
                table: "Pairs",
                column: "PairAddress",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trades_PairId",
                table: "Trades",
                column: "PairId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Trades");

            migrationBuilder.DropTable(
                name: "Pairs");
        }
    }
}
