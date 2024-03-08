using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SegmentCollection.Models
{
    public static class SegmentCollectionSegmentExtensions
    {
        public static SegmentCollectionSegment ToDto(SegmentCollectionSegmentDocument doc)
        {
            return new SegmentCollectionSegment
            {
                Id = doc.Id,
                SegmentIds = doc.SegmentIds,
                SegmentCollectionSpeed = doc.SegmentCollectionSpeed
            };
        }

        public static SegmentCollectionSegmentDocument ToDoc(SegmentCollectionSegment dto)
        {
            return new SegmentCollectionSegmentDocument
            {
                Id = dto.Id,
                SegmentIds = dto.SegmentIds,
                SegmentCollectionSpeed = dto.SegmentCollectionSpeed
            };
        }
    }
}
