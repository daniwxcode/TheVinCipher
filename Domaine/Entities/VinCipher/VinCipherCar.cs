using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json.Serialization;

using Newtonsoft.Json;

namespace Domaine.Entities.VinCipher
{
    // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
    public class VinCipherCar
    {
        [Key]
        public long Id { get; set; }
        public string? VIN { get; set; }
        [JsonPropertyName("make")]
        public string? Make { get; set; }

        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("model_year")]
        public string? ModelYear { get; set; }

        [JsonPropertyName("trim_level")]
        public string? TrimLevel { get; set; }

        [JsonPropertyName("body_style")]
        public string? BodyStyle { get; set; }

        [JsonPropertyName("engine_type")]
        public string? EngineType { get; set; }

        [JsonPropertyName("fuel_type")]
        public string? FuelType { get; set; }

        [JsonPropertyName("transmission")]
        public string? Transmission { get; set; }

        [JsonPropertyName("manufactured_in")]
        public string? ManufacturedIn { get; set; }

        [JsonPropertyName("manufacturer")]
        public string? Manufacturer { get; set; }

        [JsonPropertyName("region")]
        public string? Region { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("body_type")]
        public string? BodyType { get; set; }

        [JsonPropertyName("number_of_doors")]
        public string? NumberOfDoors { get; set; }

        [JsonPropertyName("number_of_seats")]
        public string? NumberOfSeats { get; set; }

        [JsonPropertyName("displacement_si")]
        public string? DisplacementSi { get; set; }

        [JsonPropertyName("displacement_cid")]
        public string? DisplacementCid { get; set; }

        [JsonPropertyName("displacement_nominal")]
        public string? DisplacementNominal { get; set; }

        [JsonPropertyName("engine_head")]
        public string? EngineHead { get; set; }

        [JsonPropertyName("engine_valves")]
        public string? EngineValves { get; set; }

        [JsonPropertyName("engine_cylinders")]
        public string? EngineCylinders { get; set; }

        [JsonPropertyName("engine_horse_power")]
        public string? EngineHorsePower { get; set; }

        [JsonPropertyName("engine_kilo_watts")]
        public string? EngineKiloWatts { get; set; }

        [JsonPropertyName("engine_aspiration")]
        public string? EngineAspiration { get; set; }

        [JsonPropertyName("manual_gearbox")]
        public string? ManualGearbox { get; set; }
        public string? BasePrice { get; set; }
        public DateTime DateValue { get; set; } = DateTime.Now;

        [NotMapped]
        public Dictionary<string,string> DecodedValues
        {
            get; set;
        }

        public string? VinDecodingResult
        {
            get
            {
                return JsonConvert.SerializeObject(DecodedValues);
            }
            set
            {
                DecodedValues=JsonConvert.DeserializeObject<Dictionary<string,string>>(value)??new Dictionary<string,string>();
            }
        }
        public HermesCar()
        {

        }
        public HermesCar(Dictionary<string,string> decode,string vin)
        {
            VIN = vin;
            DecodedValues=decode;
            string? _value = "";
            decode.TryGetValue("Make",out _value);
            Make=_value??"";
            decode.TryGetValue("model",out _value);
            Model=_value??"";
            decode.TryGetValue("model_year",out _value);
            ModelYear=_value??"";
            decode.TryGetValue("trim_level",out _value);
            TrimLevel=_value??"";
            decode.TryGetValue("body_style",out _value);
            BodyStyle=_value??"";
            decode.TryGetValue("engine_type",out _value);
            EngineType=_value??"";
            decode.TryGetValue("fuel_type",out _value);
            FuelType=_value??"";
            decode.TryGetValue("transmission",out _value);
            Transmission=_value??"";
            decode.TryGetValue("manufactured_in",out _value);
            ManufacturedIn=_value??"";
            decode.TryGetValue("manufacturer",out _value);
            Manufacturer=_value??"";
            decode.TryGetValue("region",out _value);
            Region=_value??"";
            decode.TryGetValue("country",out _value);
            Country=_value??"";
            decode.TryGetValue("body_type",out _value);
            BodyType=_value??"";
            decode.TryGetValue("number_of_doors",out _value);
            NumberOfDoors=_value??"";
            decode.TryGetValue("number_of_seats",out _value);
            NumberOfSeats=_value??"";
            decode.TryGetValue("displacement_si",out _value);
            DisplacementSi=_value??"";
            decode.TryGetValue("displacement_cid",out _value);
            DisplacementCid=_value??""; 
            decode.TryGetValue("displacement_nominal",out _value);
            DisplacementNominal=_value??""; 
            decode.TryGetValue("engine_head",out _value);
            EngineHead=_value??""; 
            decode.TryGetValue("engine_valves",out _value);
            EngineValves=_value??"";
            decode.TryGetValue("engine_cylinders",out _value);
            EngineCylinders=_value??"";
            decode.TryGetValue("engine_horse_power",out _value);
            EngineHorsePower=_value??"";
            decode.TryGetValue("engine_kilo_watts",out _value);
            EngineKiloWatts=_value??"";
            decode.TryGetValue("engine_aspiration",out _value);
            EngineAspiration=_value??"";
            decode.TryGetValue("manual_gearbox",out _value);
            ManualGearbox=_value??"";
            decode.TryGetValue("Base Price",out _value);
            BasePrice=_value??"";


        }
    }


}
