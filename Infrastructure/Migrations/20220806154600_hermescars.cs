using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class hermescars : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HermesCars",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VIN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Make = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModelYear = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrimLevel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BodyStyle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EngineType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FuelType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Transmission = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManufacturedIn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Manufacturer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Region = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BodyType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumberOfDoors = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumberOfSeats = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplacementSi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplacementCid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplacementNominal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EngineHead = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EngineValves = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EngineCylinders = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EngineHorsePower = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EngineKiloWatts = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EngineAspiration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManualGearbox = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BasePrice = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HermesPrice = table.Column<int>(type: "int", nullable: true),
                    DateValue = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VinDecodingResult = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HermesCars", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HermesCars");
        }
    }
}
