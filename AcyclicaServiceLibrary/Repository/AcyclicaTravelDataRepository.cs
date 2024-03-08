using Econolite.Ode.Persistence.Mongo.Context;
using Microsoft.Extensions.Logging;
using AcyclicaService.Repository.Interfaces;
using Econolite.Ode.Persistence.Mongo.Repository;

namespace AcyclicaService.Repository
{
    public class AcyclicaTravelDataRepository : GuidDocumentRepositoryBase<TravelDataDocument>, IAcyclicaTravelDataRepository
    {
        public AcyclicaTravelDataRepository(IMongoContext context, ILogger<AcyclicaTravelDataRepository> logger) : base(context, logger)
        {
        }
    }
}

