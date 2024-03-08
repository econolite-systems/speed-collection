using AcyclicaService.Repository.Interfaces;
using Econolite.Ode.Persistence.Mongo.Context;
using Econolite.Ode.Persistence.Mongo.Repository;
using Microsoft.Extensions.Logging;

namespace AcyclicaService.Repository
{
    public class AcyclicaSegmentInventoryRepository : GuidDocumentRepositoryBase<SegmentInventoryDocument>, IAcyclicaSegmentInventoryRepository

    {
        public AcyclicaSegmentInventoryRepository(IMongoContext context, ILogger<AcyclicaSegmentInventoryRepository> logger) : base(context, logger)
        {
        }
    }
}
