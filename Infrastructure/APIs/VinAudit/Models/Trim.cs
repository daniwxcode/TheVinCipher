
using System.Text.Json.Serialization;

namespace Infrastructure.APIs.VinAudit.Models
{
    public class Trim
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("selected")]
        public int Selected { get; set; }

        [JsonPropertyName("styles")]
        public List<Style> Styles { get; set; }
    }


}
