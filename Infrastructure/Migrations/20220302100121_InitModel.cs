using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class InitModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cars",
                columns: table => new
                {
                    Vin = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Make = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    EnginPower = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Transmission = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Energy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EngineCylender = table.Column<double>(type: "float", nullable: false),
                    Doors = table.Column<int>(type: "int", nullable: false),
                    Seats = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Trim = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Size = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MarketValue = table.Column<int>(type: "int", nullable: false),
                    ValueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MadeDeate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "CarsBases",
                columns: table => new
                {
                    Vin = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: true),
                    Make = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Trim = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Series = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Style = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Size = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MadeIn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MadeInCity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Doors = table.Column<int>(type: "int", nullable: true),
                    FuelType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FuelCapacity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SequentialNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CityMileage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HighwayMileage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Engine = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EngineSize = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EngineCylinders = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Transmission = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransmissionType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransmissionSpeeds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Drivetrain = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AntiBrakeSystem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SteeringType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurbWeight = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WeightEmptykg = table.Column<int>(type: "int", nullable: true),
                    GrossVehicleWeightRating = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OverallHeight = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OverallLength = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OverallWidth = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WheelbaseLength = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StandardSeating = table.Column<int>(type: "int", nullable: true),
                    HermesMarketValue = table.Column<int>(type: "int", nullable: false),
                    Devise = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ManufacturerSuggestedRetailPrice = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MadeDeate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarsBases", x => x.Vin);
                });

            migrationBuilder.CreateTable(
                name: "Requests",
                columns: table => new
                {
                    Vin = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsManaged = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requests", x => x.Vin);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cars");

            migrationBuilder.DropTable(
                name: "CarsBases");

            migrationBuilder.DropTable(
                name: "Requests");
        }
    }
}
