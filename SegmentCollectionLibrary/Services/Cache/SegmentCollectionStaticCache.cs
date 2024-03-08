using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SegmentCollection.Models;

namespace SegmentCollection.Services.Cache
{
    public class SegmentCollectionStaticCache : ISegmentCollectionCache
    {
        List<SegmentCollectionSegment> _segments;

        public SegmentCollectionStaticCache()
        {
            _segments = new List<SegmentCollectionSegment>();
        }

        public Task GetSegmentCollectionCacheAsync()
        {
            return Task.FromResult(_segments);
        }

        public Task UpdateSegmentCollectionCacheAsync(List<SegmentCollectionSegment> segments)
        {
            return Task.Run(() =>
            {
                _segments = segments;
            });
        }
    }
}
