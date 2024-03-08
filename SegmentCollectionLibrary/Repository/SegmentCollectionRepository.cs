using Econolite.Ode.Persistence.Mongo.Context;
using Econolite.Ode.Persistence.Mongo.Repository;
using Microsoft.Extensions.Logging;
using SegmentCollection.Models;
using SegmentCollection.Repository.Interfaces;

namespace SegmentCollection.Repository
{
    public class SegmentCollectionRepository : GuidDocumentRepositoryBase<SegmentCollectionSegmentDocument>, ISegmentCollectionRepository
    {
        public SegmentCollectionRepository(IMongoContext context, ILogger<SegmentCollectionRepository> logger) : base(context, logger)
        {
        }
    }
}
