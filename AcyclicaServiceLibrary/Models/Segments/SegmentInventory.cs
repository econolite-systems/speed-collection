using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AcyclicaService.Models.Segments
{
    public class SegmentInventory
    {
        public Guid Id { get; set; } = default!;

        [JsonPropertyName("segment_id")]
        public int SegmentId { get; set; }

        [JsonPropertyName("start_serial")]
        public long StartSerial { get; set; }

        [JsonPropertyName("start_id")]
        public int StartId { get; set; }

        [JsonPropertyName("end_serial")]
        public long EndSerial { get; set; }

        [JsonPropertyName("end_id")]
        public int EndId { get; set; }

        [JsonPropertyName("distance")]
        [BsonElement("Distance")]
        public double Distance { get; set; }

        [JsonPropertyName("thresholds")]
        public List<List<Threshold>> Thresholds { get; set; } = default!;

        [JsonPropertyName("polyline")]

        public string Polyline { get; set; } = default!;

        [JsonPropertyName("base_offset")]
        public int BaseOffset { get; set; }

        [JsonPropertyName("start_location_id")]
        public int StartLocationId { get; set; }

        [JsonPropertyName("start_location_name")]
        public string StartLocationName { get; set; } = default!;

        [JsonPropertyName("end_location_name")]
        public string EndLocationName { get; set; } = default!;

        [JsonPropertyName("end_location_id")]
        public int EndLocationId { get; set; }
    }
}
