
using System.Text.Json.Serialization;

namespace Infrastructure.APIs.VinAudit.Models
{

    public class VinAuditResponse
    {
        [JsonPropertyName("input")]
        public Input Input { get; set; }

        [JsonPropertyName("selections")]
        public Selections Selections { get; set; }

        [JsonPropertyName("attributes")]
        public Attributes Attributes { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("error")]
        public string Error { get; set; }
    }


}
