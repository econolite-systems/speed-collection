using AcyclicaService.Models.Segments;
using AcyclicaService.Models.TravelTime;
using AcyclicaService.Models.LocationInventory;
using AcyclicaService.Repository;
using EntityService;
using Pipelines.Sockets.Unofficial.Arenas;
using System.Runtime.CompilerServices;
using AcyclicaService.Extensions;
using Econolite.Ode.Status.Speed;
using Econolite.Ode.Status.Common;

namespace AcyclicaService.Helpers
{
    public static class AcyclicaServiceExtensions
    {
        public static long GetStartTimeEpoch(int intervalInSeconds)
        {
            DateTime startTime = DateTime.UtcNow.AddSeconds(-intervalInSeconds);
            long epochTime = (long)(startTime - new DateTime(1970, 1, 1)).TotalSeconds;

            return epochTime;
        }

        public static SegmentInventory ToDto(this SegmentInventoryDocument segmentInventory)
        {
            return new SegmentInventory
            {
                Id = segmentInventory.Id,
                SegmentId = segmentInventory.SegmentId,
                BaseOffset = segmentInventory.BaseOffset,
                Distance = segmentInventory.Distance,
                EndId = segmentInventory.EndId,
                EndSerial = segmentInventory.EndSerial,
                Polyline = segmentInventory.Polyline,
                StartId = segmentInventory.StartId,
                StartSerial = segmentInventory.StartSerial,
                Thresholds = segmentInventory.Thresholds.Select(tList => tList.Select(t => t.ToDto()).ToList()).ToList()
            };

        }

        public static double ConvertSegmentTravelTimeFromMillisecondsIntoMph(double distance, long time)
        {
            var speed = 0.0;
            if (distance > 0 && time > 0)
                speed = distance / ((double)time / 3600000);

            return speed;
        }

        public static EntityNode ToEntityNode(this SegmentInventory segment, EntityType entityType, EntityNode parent, LocationInventory endLocation)
        {
            var decodedPolyline = GooglePoints.Decode(segment.Polyline).ToArray();
            var entityNode = new EntityNode()
            {
                Id = Guid.NewGuid(),
                Name = $"Segment Id {segment.SegmentId}",
                Password = "",
                Username = "",
                IpAddress = "",
                Description = "",
                PrivacyPhrase = "",

                Type = new EntityTypeId()
                {
                    Id = entityType.Id,
                    Name = entityType.Name
                },
                Parents = new List<Guid>()
                            {
                                parent.Id
                            },
                Parent = parent.Id,
                Children = new List<Entity>()
                {

                },
                Geometry = new GeoJsonGeometry()
                {
                    Point = new GeoJsonPointFeature()
                    {
                        Coordinates = new double[] { endLocation.Longitude, endLocation.Latitude },
                        Type = GeoSpatialType.Point
                    },
                    LineString = new GeoJsonLineStringFeature()
                    {
                        Coordinates = decodedPolyline,
                        Type = GeoSpatialType.LineString,
                        Properties = new GeoJsonProperties()
                        {
                            SpeedLimit = 0,
                            TripPointLocations = new List<TripPointLocation>()
                                        {
                                            new TripPointLocation()
                                            {
                                                Point = new List<double>()
                                                {
                                                    segment.Distance
                                                }
                                            }
                                        }
                        }
                    }
                },
                IdMapping = segment.SegmentId,
                IsLeaf = true,

            };
            return entityNode;
        }

        public static bool HasChanged(this EntityNode entityNode, SegmentInventory segment)
        {
            if (entityNode.Geometry?.LineString?.Coordinates == null)
            {
                return true;
            }
            var encodedPolyline = GooglePoints.Encode(entityNode.Geometry.LineString.Coordinates.Select(c => new double[]{c.First(), c.Last()}));
            return segment.Polyline != encodedPolyline;
        }
        
        public static EntityNode Update(this EntityNode entityNode, SegmentInventory segment, LocationInventory location)
        {
            var decodedPolyline = GetDecodedPolyline(segment);

            entityNode.Geometry.Point.Coordinates = new double[] { location.Longitude, location.Latitude };
            entityNode.Geometry.LineString.Coordinates = decodedPolyline;
            entityNode.Geometry.LineString.Properties.TripPointLocations = new List<TripPointLocation>()
                                        {
                                            new TripPointLocation()
                                            {
                                                Point = new List<double>()
                                                {
                                                    segment.Distance
                                                }
                                            }
                                        };

            return entityNode;
        }

        public static SpeedEvent ToSpeedEvent(this EntityNode segment)
        {
            var coordinates = segment.Geometry.LineString.Coordinates.Select(c => c.ToArray())
                .Select(a => new double[] {a[0], a[1]}).ToArray();
            return new SpeedEvent()
            {
                DeviceId = segment.Id,
                CommStatus = CommStatus.Unknown,
                SegmentId = segment.IdMapping ?? 0,
                Latitude = segment.Geometry.Point.Coordinates.Last(),
                Longitude = segment.Geometry.Point.Coordinates.First(),
                PolylineCoordinates = coordinates,
                SegmentSpeed = 0,
                TimeStamp = DateTime.Now
            };
        }
        
        public static SpeedEvent CreateSpeedEvent(this SegmentInventory segment, LocationInventory endLocation)
        {
            var decodedPolyline = GetDecodedPolyline(segment);

            return new SpeedEvent()
            {
                DeviceId = Guid.NewGuid(),
                CommStatus = CommStatus.Unknown,
                SegmentId = segment.SegmentId,
                Latitude = endLocation.Latitude,
                Longitude = endLocation.Longitude,
                PolylineCoordinates = decodedPolyline,
                SegmentSpeed = 0,
                TimeStamp = DateTime.Now
            };
        }

        public static double[][] GetDecodedPolyline(SegmentInventory segment) => GooglePoints.Decode(segment.Polyline).ToArray();

        #region DocAndDtoConversions
        public static SegmentInventoryDocument ToDoc(this SegmentInventory segmentInventoryDto)
        {
            return new SegmentInventoryDocument
            {
                Id = segmentInventoryDto.Id,
                SegmentId = segmentInventoryDto.SegmentId,
                BaseOffset = segmentInventoryDto.BaseOffset,
                Distance = segmentInventoryDto.Distance,
                EndId = segmentInventoryDto.EndId,
                EndSerial = segmentInventoryDto.EndSerial,
                Polyline = segmentInventoryDto.Polyline,
                StartId = segmentInventoryDto.StartId,
                StartSerial = segmentInventoryDto.StartSerial,
                Thresholds = segmentInventoryDto.Thresholds.Select(tList => tList.Select(t => t.ToDoc()).ToList()).ToList()
            };
        }

        public static Threshold ToDto(this ThresholdDocument threshold)
        {
            return new Threshold
            {
                Color = threshold.Color,
                Value = threshold.Value
            };
        }

        public static ThresholdDocument ToDoc(this Threshold thresholdDto)
        {
            return new ThresholdDocument
            {
                Color = thresholdDto.Color,
                Value = thresholdDto.Value
            };
        }

        public static TravelData ToDto(this TravelDataDocument travelData)
        {
            return new TravelData
            {
                Id = travelData.Id,
                SegmentId = travelData.SegmentId,
                Time = travelData.Time,
                Strength = travelData.Strength,
                First = travelData.First,
                Last = travelData.Last,
                Minimum = travelData.Minimum,
                Maximum = travelData.Maximum,
            };
        }

        public static TravelDataDocument ToDoc(this TravelData travelDataDto)
        {
            return new TravelDataDocument
            {
                Id = travelDataDto.Id,
                SegmentId = travelDataDto.SegmentId,
                Time = travelDataDto.Time,
                Strength = travelDataDto.Strength,
                First = travelDataDto.First,
                Last = travelDataDto.Last,
                Minimum = travelDataDto.Minimum,
                Maximum = travelDataDto.Maximum
            };
        }

        public static LocationInventoryDocument ToDoc(this LocationInventory locationInventory)
        {
            return new LocationInventoryDocument()
            {
                LocationId = locationInventory.LocationId,
                Latitude = locationInventory.Latitude,
                Longitude = locationInventory.Longitude,
                DiffrfSensors = locationInventory.DiffrfSensors,
                Cabinets = locationInventory.Cabinets,
                VsoSensors = locationInventory.VsoSensors,
                UserFiles = locationInventory.UserFiles
            };
        }

        public static LocationInventory ToDto(this LocationInventoryDocument locationInventory)
        {
            return new LocationInventory()
            {
                LocationId = locationInventory.LocationId,
                Latitude = locationInventory.Latitude,
                Longitude = locationInventory.Longitude,
                DiffrfSensors = locationInventory.DiffrfSensors,
                Cabinets = locationInventory.Cabinets,
                VsoSensors = locationInventory.VsoSensors,
                UserFiles = locationInventory.UserFiles
            };
        }
        #endregion
    }

}