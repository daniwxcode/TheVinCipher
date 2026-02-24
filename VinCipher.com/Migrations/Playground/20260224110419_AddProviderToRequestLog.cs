using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinCipher.com.Migrations.Playground
{
    /// <inheritdoc />
    public partial class AddProviderToRequestLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "RequestLogs",
                type: "TEXT",
                maxLength: 30,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Provider",
                table: "RequestLogs");
        }
    }
}
