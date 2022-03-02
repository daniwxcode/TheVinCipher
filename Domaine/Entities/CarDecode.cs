using Domaine.Abstracts;

using System.ComponentModel.DataAnnotations;

namespace Domaine.Entities
{
    public class CarDecode : BaseEntity
    {
        public string Vin { get; set; }
        public int? Year { get; set; } = 0;
        public string? Make { get; set; }
        public string? Model { get; set; }
        public string? Trim { get; set; }
        public string? Series { get; set; }
        public string? Style { get; set; }
        public string? Type { get; set; }
        public string? Size { get; set; }
        public string? Category { get; set; }
        public string? MadeIn { get; set; }
        public string? MadeInCity { get; set; }
        public int? Doors { get; set; }
        public string? FuelType { get; set; }
        public string? FuelCapacity { get; set; }
        public string? SequentialNumber { get; set; }
        public string? CityMileage { get; set; }
        public string? HighwayMileage { get; set; }
        public string? Engine { get; set; }
        public string? EngineSize { get; set; }
        public string? EngineCylinders { get; set; }
        public string? Transmission { get; set; }
        public string? TransmissionType { get; set; }
        public string? TransmissionSpeeds { get; set; }
        public string? Drivetrain { get; set; }
        public string? AntiBrakeSystem { get; set; }
        public string? SteeringType { get; set; }
        public string? CurbWeight { get; set; }
        public int? WeightEmptykg { get; set; }
        public string? GrossVehicleWeightRating { get; set; }
        public string? OverallHeight { get; set; }
        public string? OverallLength { get; set; }
        public string? OverallWidth { get; set; }
        public string? WheelbaseLength { get; set; }
        public int? StandardSeating { get; set; }
        public int HermesMarketValue { get; set; } = 0;
        public string Devise { get; set; } = "XOF";

        public CarDecode (CarBase carBase)
        {
            Vin = carBase.Vin;
            Year = carBase.Year;
            Make = carBase.Make;
            MadeIn = carBase.MadeIn;
            Model = carBase.Model;
            Devise = carBase.Devise;
            HermesMarketValue = carBase.HermesMarketValue;
            StandardSeating = carBase.StandardSeating;
            WheelbaseLength = carBase.WheelbaseLength;
            OverallHeight = carBase.OverallHeight;
            OverallLength = carBase.OverallLength;
            Trim = carBase.Trim;
            Series = carBase.Series;
            Style = carBase.Style;
            Type = carBase.Type;
            Size = carBase.Size;
            Category = carBase.Category;
            MadeInCity = carBase.MadeInCity;
            Doors = carBase.Doors;
            FuelCapacity = carBase.FuelCapacity;
            FuelType = carBase.FuelType;
            SequentialNumber = carBase.SequentialNumber;
            CityMileage = carBase.CityMileage;
            HighwayMileage = carBase.HighwayMileage;
            Engine = carBase.Engine;
            EngineSize = carBase.EngineSize;
            EngineCylinders = carBase.EngineCylinders;
            Transmission = carBase.Transmission;
            TransmissionType = carBase.TransmissionType;
            TransmissionSpeeds = carBase.TransmissionSpeeds;
            Drivetrain = carBase.Drivetrain;
            AntiBrakeSystem = carBase.AntiBrakeSystem;
            SteeringType = carBase.SteeringType;
            CurbWeight = carBase.CurbWeight;
            WeightEmptykg = carBase.WeightEmptykg;
            GrossVehicleWeightRating = carBase.GrossVehicleWeightRating;
            OverallWidth = carBase.OverallWidth;






        }
    }
}
