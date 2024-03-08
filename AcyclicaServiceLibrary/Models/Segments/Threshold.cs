using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace AcyclicaService.Models.Segments
{
    public class Threshold
    {
        [JsonPropertyName("color")]
        public string Color { get; set; } = default!;

        [JsonPropertyName("value")]
        public int Value { get; set; }
    }

}