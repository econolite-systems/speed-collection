// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Status.Common;
using Econolite.Ode.Status.Speed;

namespace Econolite.Ode.Models.SpeedStatus
{
    public class SpeedStatusModel
    {
        public Guid DeviceId { get; set; }
        public string ActionEventType { get; set; } = string.Empty;
        public DateTime TimeStamp { get; set; }
        public int SegmentId { get; set; }
        public double SegmentSpeed { get; set; } = 0.0;
        public CommStatus CommStatus { get; set; } = default!;
        public TrafficStatus TrafficStatus { get; set; } = TrafficStatus.Normal;
        public double Latitude { get; set; } = 0.0;
        public double Longitude { get; set; } = 0.0;
        public string Location { get; set; } = string.Empty;
        public double[][] Coordinates { get; set; } = default!;
    }
}