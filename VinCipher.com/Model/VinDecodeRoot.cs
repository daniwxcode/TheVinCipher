using System.Text.Json.Serialization;

namespace VinCipher.Model
{
    // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
    public class Result
    {
        [JsonPropertyName("Value")]
        public string Value { get; set; }

        [JsonPropertyName("ValueId")]
        public string ValueId { get; set; }

        [JsonPropertyName("Variable")]
        public string Variable { get; set; }

        [JsonPropertyName("VariableId")]
        public int VariableId { get; set; }
    }

    public class VinDecodeRoot
    {
        [JsonPropertyName("Count")]
        public int Count { get; set; }

        [JsonPropertyName("Message")]
        public string Message { get; set; }

        [JsonPropertyName("SearchCriteria")]
        public string SearchCriteria { get; set; }

        [JsonPropertyName("Results")]
        public List<Result> Results { get; set; }
    }


}
