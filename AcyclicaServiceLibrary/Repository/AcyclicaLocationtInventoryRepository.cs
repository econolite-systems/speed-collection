using AcyclicaService.Repository.Interfaces;
using Econolite.Ode.Persistence.Mongo.Context;
using Econolite.Ode.Persistence.Mongo.Repository;
using Microsoft.Extensions.Logging;

namespace AcyclicaService.Repository
{
    public class AcyclicaLocationInventoryRepository : GuidDocumentRepositoryBase<LocationInventoryDocument>, IAcyclicaLocationInventoryRepository
    {
        public AcyclicaLocationInventoryRepository(IMongoContext context, ILogger<AcyclicaLocationInventoryRepository> logger) : base(context, logger)
        {
        }
    }
}
