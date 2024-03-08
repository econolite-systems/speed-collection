// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Status.Speed;

namespace Econolite.Ode.Repository.SpeedStatus
{
    public interface ISpeedStatusRepository
    {
        public Task<SpeedEvent?> GetAsync(Guid deviceId);
        public Task<SpeedEvent?[]> GetAllAsync();
        Task PutStatusAsync(Guid tenantId, Guid deviceId, SpeedEvent speedEvent, CancellationToken cancellationToken = default);
    }
}