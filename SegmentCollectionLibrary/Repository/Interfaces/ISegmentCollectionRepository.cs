using Econolite.Ode.Persistence.Common.Repository;
using SegmentCollection.Models;

namespace SegmentCollection.Repository.Interfaces
{
    public interface ISegmentCollectionRepository : IRepository<SegmentCollectionSegmentDocument, Guid>
    {
    }
}