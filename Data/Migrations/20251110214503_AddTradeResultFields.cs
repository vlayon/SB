using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTradeResultFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AmountDimension",
                table: "Trades",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "Trades",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "GasUsed",
                table: "Trades",
                type: "decimal(18,8)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "Success",
                table: "Trades",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TokenAddressIn",
                table: "Trades",
                type: "nvarchar(42)",
                maxLength: 42,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TokenAddressOut",
                table: "Trades",
                type: "nvarchar(42)",
                maxLength: 42,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TransactionHash",
                table: "Trades",
                type: "nvarchar(66)",
                maxLength: 66,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountDimension",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "GasUsed",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "Success",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "TokenAddressIn",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "TokenAddressOut",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "TransactionHash",
                table: "Trades");
        }
    }
}
