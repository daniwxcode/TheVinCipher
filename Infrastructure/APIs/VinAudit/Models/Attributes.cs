
using System.Text.Json.Serialization;

namespace Infrastructure.APIs.VinAudit.Models
{
    public class Attributes
    {
        [JsonPropertyName("year")]
        public string Year { get; set; }

        [JsonPropertyName("make")]
        public string Make { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("trim")]
        public string Trim { get; set; }

        [JsonPropertyName("style")]
        public string Style { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("size")]
        public string Size { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("made_in")]
        public string MadeIn { get; set; }

        [JsonPropertyName("made_in_city")]
        public string MadeInCity { get; set; }

        [JsonPropertyName("doors")]
        public string Doors { get; set; }

        [JsonPropertyName("fuel_type")]
        public string FuelType { get; set; }

        [JsonPropertyName("fuel_capacity")]
        public string FuelCapacity { get; set; }

        [JsonPropertyName("city_mileage")]
        public string CityMileage { get; set; }

        [JsonPropertyName("highway_mileage")]
        public string HighwayMileage { get; set; }

        [JsonPropertyName("engine")]
        public string Engine { get; set; }

        [JsonPropertyName("engine_size")]
        public string EngineSize { get; set; }

        [JsonPropertyName("engine_cylinders")]
        public string EngineCylinders { get; set; }

        [JsonPropertyName("transmission")]
        public string Transmission { get; set; }

        [JsonPropertyName("transmission_type")]
        public string TransmissionType { get; set; }

        [JsonPropertyName("transmission_speeds")]
        public string TransmissionSpeeds { get; set; }

        [JsonPropertyName("drivetrain")]
        public string Drivetrain { get; set; }

        [JsonPropertyName("anti_brake_system")]
        public string AntiBrakeSystem { get; set; }

        [JsonPropertyName("steering_type")]
        public string SteeringType { get; set; }

        [JsonPropertyName("curb_weight")]
        public string CurbWeight { get; set; }

        [JsonPropertyName("gross_vehicle_weight_rating")]
        public string GrossVehicleWeightRating { get; set; }

        [JsonPropertyName("overall_height")]
        public string OverallHeight { get; set; }

        [JsonPropertyName("overall_length")]
        public string OverallLength { get; set; }

        [JsonPropertyName("overall_width")]
        public string OverallWidth { get; set; }

        [JsonPropertyName("wheelbase_length")]
        public string WheelbaseLength { get; set; }

        [JsonPropertyName("standard_seating")]
        public string StandardSeating { get; set; }

        [JsonPropertyName("invoice_price")]
        public string InvoicePrice { get; set; }

        [JsonPropertyName("delivery_charges")]
        public string DeliveryCharges { get; set; }

        [JsonPropertyName("manufacturer_suggested_retail_price")]
        public string ManufacturerSuggestedRetailPrice { get; set; }
    }


}
