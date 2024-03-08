using AcyclicaService.Helpers;
using AcyclicaService.Services.Cache;
using AcyclicaService.Services.Database;
using Econolite.Ode.Status.Common;
using Econolite.Ode.Status.Speed;
using EntityService;
using SegmentCollection.Models;

namespace SegmentCollection.Extensions
{
    public static class SegmentCollectionServiceExtensions
    {
        public static SegmentCollectionSegment ToDto(this SegmentCollectionSegmentDocument document)
        {
            return new SegmentCollectionSegment()
            {
                SegmentIds = document.SegmentIds,
                SegmentCollectionSpeed = document.SegmentCollectionSpeed,
            };
        }

        public static SegmentCollectionSegmentDocument ToDoc(this SegmentCollectionSegment segment)
        {
            return new SegmentCollectionSegmentDocument()
            {
                SegmentIds = segment.SegmentIds,
                SegmentCollectionSpeed = segment.SegmentCollectionSpeed,
            };
        }

        public static List<SegmentCollectionSegment> CreateLongQueueFromSegments(List<EntityNode> sortedentityNodes,
     int segmentCollectionLength)
        {
            var segmentCollectionSegment = new SegmentCollectionSegment()
            {
                Id = Guid.NewGuid(),
                SegmentIds = new List<int?>()
            };
            double averageSpeed = 0;
            var segmentCollectionList = new List<SegmentCollectionSegment>();
            int segmentLengthCount = 0;

            foreach (var entityNode in sortedentityNodes)
            {
                segmentCollectionSegment.SegmentIds.Add(entityNode.IdMapping);
                if ((entityNode.Geometry?.LineString?.Properties?.SpeedLimit ?? 0) > 0)
                {
                    averageSpeed += entityNode.Geometry.LineString.Properties.SpeedLimit!.Value;
                }

                if (segmentCollectionSegment.SegmentIds.Count == segmentCollectionLength || segmentLengthCount == sortedentityNodes.Count - 1)
                {
                    segmentCollectionSegment.SegmentCollectionSpeed =
                        averageSpeed / segmentCollectionSegment.SegmentIds.Count;
                    if (segmentCollectionSegment.SegmentCollectionSpeed <= 30)
                    {
                        segmentCollectionList.Add(segmentCollectionSegment);
                    }

                    segmentCollectionSegment = new SegmentCollectionSegment()
                    {
                        Id = Guid.NewGuid(),
                        SegmentIds = new List<int?>()
                    };
                    averageSpeed = 0;
                }
                segmentLengthCount++;
            }

            return segmentCollectionList;
        }


    }
}
