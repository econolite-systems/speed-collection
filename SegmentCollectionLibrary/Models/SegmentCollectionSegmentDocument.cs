using Econolite.Ode.Persistence.Common.Interfaces;

namespace SegmentCollection.Models
{
    public class SegmentCollectionSegmentDocument : IIndexedEntity<Guid>
    {
        public Guid Id { get; set; }
        public List<int?> SegmentIds { get; set; } = default!;
        public double SegmentCollectionSpeed { get; set; }
    }
}