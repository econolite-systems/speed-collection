using Econolite.Ode.Persistence.Common.Interfaces;

namespace AcyclicaService.Repository
{
    public class SegmentInventoryDocument : IIndexedEntity<Guid>
    {
        public Guid Id { get; set; }
        public int SegmentId { get; set; }
        public long StartSerial { get; set; }
        public int StartId { get; set; }
        public long EndSerial { get; set; }
        public int EndId { get; set; }
        public double Distance { get; set; }
        public List<List<ThresholdDocument>> Thresholds { get; set; } = default!;
        public string Polyline { get; set; } = default!;
        public int BaseOffset { get; set; }
    }
    public class ThresholdDocument
    {
        public string Color { get; set; } = default!;
        public int Value { get; set; }
    }
}
