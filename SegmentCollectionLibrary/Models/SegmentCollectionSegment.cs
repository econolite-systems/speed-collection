namespace SegmentCollection.Models
{
    public class SegmentCollectionSegment
    {
        public Guid Id { get; set; }
        public List<int?> SegmentIds { get; set; } = default!;
        public double SegmentCollectionSpeed { get; set; }
    }
}