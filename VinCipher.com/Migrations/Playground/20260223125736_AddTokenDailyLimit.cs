using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinCipher.com.Migrations.Playground
{
    /// <inheritdoc />
    public partial class AddTokenDailyLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DailyLimit",
                table: "ApiTokens",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyLimit",
                table: "ApiTokens");
        }
    }
}
