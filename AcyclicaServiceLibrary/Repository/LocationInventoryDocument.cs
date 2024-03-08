using Econolite.Ode.Persistence.Common.Interfaces;

namespace AcyclicaService.Repository
{
    public class LocationInventoryDocument : IIndexedEntity<Guid>
    {
        public Guid Id { get; set; }
        public int LocationId { get; set; } = default!;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public List<int> DiffrfSensors { get; set; } = default!;
        public List<int> Cabinets { get; set; } = default!;
        public List<int> VsoSensors { get; set; } = default!;
        public List<int> UserFiles { get; set; } = default!;
    }
}
