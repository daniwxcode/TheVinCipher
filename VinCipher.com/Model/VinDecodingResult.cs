using System.Text.Json.Serialization;

namespace VinCipher.Model
{
    public class VinDecodingResult
    {
        [JsonPropertyName("manufacturer")]
        public string Manufacturer { get; set; }

        [JsonPropertyName("brand")]
        public string Brand { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("image")]
        public string Image { get; set; }

        [JsonPropertyName("year")]
        public string Year { get; set; }

        [JsonPropertyName("serial")]
        public string Serial { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("exec")]
        public string Exec { get; set; }

        [JsonPropertyName("mileage")]
        public string Mileage { get; set; }

        [JsonPropertyName("body_type")]
        public string BodyType { get; set; }

        [JsonPropertyName("number_of_seater")]
        public string NumberOfSeater { get; set; }

        [JsonPropertyName("length")]
        public string Length { get; set; }

        [JsonPropertyName("width")]
        public string Width { get; set; }

        [JsonPropertyName("height")]
        public string Height { get; set; }

        [JsonPropertyName("wheelbase")]
        public string Wheelbase { get; set; }

        [JsonPropertyName("front_track")]
        public string FrontTrack { get; set; }

        [JsonPropertyName("rear_track")]
        public string RearTrack { get; set; }

        [JsonPropertyName("engine_type")]
        public string EngineType { get; set; }

        [JsonPropertyName("capacity")]
        public string Capacity { get; set; }

        [JsonPropertyName("engine_power")]
        public string EnginePower { get; set; }

        [JsonPropertyName("max_power_at_rpm")]
        public string MaxPowerAtRpm { get; set; }

        [JsonPropertyName("maximum_torque")]
        public string MaximumTorque { get; set; }

        [JsonPropertyName("injection_type")]
        public string InjectionType { get; set; }

        [JsonPropertyName("cylinder_layout")]
        public string CylinderLayout { get; set; }

        [JsonPropertyName("number_of_cylinders")]
        public string NumberOfCylinders { get; set; }

        [JsonPropertyName("fuel")]
        public string Fuel { get; set; }

        [JsonPropertyName("gearbox_type")]
        public string GearboxType { get; set; }

        [JsonPropertyName("number_of_gear")]
        public string NumberOfGear { get; set; }

        [JsonPropertyName("drive_wheels")]
        public string DriveWheels { get; set; }

        [JsonPropertyName("front_brakes")]
        public string FrontBrakes { get; set; }

        [JsonPropertyName("rear_brakes")]
        public string RearBrakes { get; set; }

        [JsonPropertyName("curb_weight")]
        public string CurbWeight { get; set; }

        [JsonPropertyName("fuel_tank_capacity")]
        public string FuelTankCapacity { get; set; }

        [JsonPropertyName("ground_clearance")]
        public string GroundClearance { get; set; }

        [JsonPropertyName("valves_per_cylinder")]
        public string ValvesPerCylinder { get; set; }

        [JsonPropertyName("front_suspension")]
        public string FrontSuspension { get; set; }

        [JsonPropertyName("back_suspension")]
        public string BackSuspension { get; set; }

        [JsonPropertyName("max_trunk_capacity")]
        public string MaxTrunkCapacity { get; set; }

        [JsonPropertyName("min_trunk_capacity")]
        public string MinTrunkCapacity { get; set; }

        [JsonPropertyName("cylinder_bore")]
        public string CylinderBore { get; set; }

        [JsonPropertyName("stroke_cycle")]
        public string StrokeCycle { get; set; }

        [JsonPropertyName("city_driving_fuel_consumption_per_100_km")]
        public string CityDrivingFuelConsumptionPer100Km { get; set; }

        [JsonPropertyName("highway_driving_fuel_consumption_per_100_km")]
        public string HighwayDrivingFuelConsumptionPer100Km { get; set; }

        [JsonPropertyName("turning_circle")]
        public string TurningCircle { get; set; }

        [JsonPropertyName("full_weight")]
        public string FullWeight { get; set; }

        [JsonPropertyName("cruising_range")]
        public string CruisingRange { get; set; }

        [JsonPropertyName("turnover_of_maximum_torque")]
        public string TurnoverOfMaximumTorque { get; set; }
    }
}
