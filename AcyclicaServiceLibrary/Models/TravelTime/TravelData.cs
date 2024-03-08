using MongoDB.Bson;
using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace AcyclicaService.Models.TravelTime
{
    public class TravelData
    {
        public Guid Id { get; set; } = default!;

        [JsonPropertyName("segment_id")]
        public int SegmentId { get; set; }

        public long Time { get; set; }

        public int Strength { get; set; }

        public int First { get; set; }

        public int Last { get; set; }

        public int Minimum { get; set; }

        public int Maximum { get; set; }
    }
}
