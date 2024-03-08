// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Models.SpeedStatus;
using Econolite.Ode.Status.Speed;

namespace Econolite.Ode.Extensions.SpeedStatus
{
    public static class SpeedStatusExtensions
    {
        public static SpeedStatusModel ToModel(this SpeedEvent status)
        {
            // Flip Longitude and Latitude around to be Latitude and Longitude
            if (status.PolylineCoordinates != null && status.PolylineCoordinates.Length > 0)
            {
                Array.ForEach(status.PolylineCoordinates, coordinates =>
                {
                    Array.Reverse(coordinates);
                });
            }

            return new SpeedStatusModel
            {
                DeviceId = status.DeviceId,
                ActionEventType = status.ActionEventType,
                TimeStamp = status.TimeStamp,
                SegmentId = status.SegmentId,
                SegmentSpeed = status.SegmentSpeed,
                CommStatus = status.CommStatus,
                TrafficStatus = status.TrafficStatus,
                Latitude = status.Latitude,
                Longitude = status.Longitude,
                Location = status.Location,
                Coordinates = status.PolylineCoordinates
            };
        }
    }
}