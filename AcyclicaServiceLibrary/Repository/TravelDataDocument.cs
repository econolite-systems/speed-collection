using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Econolite.Ode.Persistence.Common.Interfaces;

namespace AcyclicaService.Repository
{
    public class TravelDataDocument : IIndexedEntity<Guid>
    {
        public Guid Id { get; set; }
        public int SegmentId { get; set; }
        public long Time { get; set; }
        public int Strength { get; set; }
        public int First { get; set; }
        public int Last { get; set; }
        public int Minimum { get; set; }
        public int Maximum { get; set; }
    }
}
