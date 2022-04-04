
using System.Text.Json.Serialization;

namespace Infrastructure.APIs.VinAudit.Models
{
    public class Input
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("vin")]
        public string Vin { get; set; }

        [JsonPropertyName("format")]
        public string Format { get; set; }
    }


}
