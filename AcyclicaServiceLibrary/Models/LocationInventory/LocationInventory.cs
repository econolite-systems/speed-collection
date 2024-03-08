using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace AcyclicaService.Models.LocationInventory
{
    public class Location
    {
        [JsonPropertyName("locations")]
        public List<LocationInventory> Locations { get; set; } = default!;
    }
    public class LocationInventory
    {
        public Guid UniqueId { get; set; } = default!;

        [JsonPropertyName("id")]
        public int LocationId { get; set; } = default!;

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("diffrfsensors")]
        public List<int> DiffrfSensors { get; set; } = default!;

        [JsonPropertyName("cabinets")]
        public List<int> Cabinets { get; set; } = default!;

        [JsonPropertyName("vsosensors")]
        public List<int> VsoSensors { get; set; } = default!;

        [JsonPropertyName("userfiles")]
        public List<int> UserFiles { get; set; } = default!;
    }
}
