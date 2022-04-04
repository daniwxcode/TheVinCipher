
using System.Text.Json.Serialization;

namespace Infrastructure.APIs.VinAudit.Models
{
    public class Selections
    {
        [JsonPropertyName("trims")]
        public List<Trim> Trims { get; set; }
    }


}
