using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SegmentCollection.Services.Cache
{
    public class SegmentCollectionCacheOptions
    {
        public TimeSpan CacheRefreshPeriod { get; set; } = TimeSpan.FromMinutes(15);
    }
}
