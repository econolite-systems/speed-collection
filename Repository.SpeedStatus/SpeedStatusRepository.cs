// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Status.Speed;
using Microsoft.Extensions.Configuration;
using ProtoBuf;
using StackExchange.Redis;
using System.Text.Json;

namespace Econolite.Ode.Repository.SpeedStatus
{
    public class SpeedStatusRepository : ISpeedStatusRepository
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _redisDb;
        private readonly Guid _siteId = Guid.Empty;

        public SpeedStatusRepository(IConfiguration configuration)
        {
            _redis = ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis") ?? throw new NullReferenceException("ConnectionStrings:Redis not found in configuration"));
            _redisDb = _redis.GetDatabase();
        }

        public async Task<SpeedEvent?> GetAsync(Guid deviceId)
        {
            var fullKey = GetFullKey(_siteId);
            var result = await _redisDb.HashGetAsync(fullKey, deviceId.ToString());
            if (!result.HasValue || result.IsNullOrEmpty)
            {
                return null;
            }
            var status = JsonSerializer.Deserialize<SpeedEvent>(result!);
            return status;
        }

        public async Task<SpeedEvent?[]> GetAllAsync()
        {
            var fullKey = GetFullKey(_siteId);
            var result = await _redisDb.HashGetAllAsync(fullKey);
            return result.Select(s => JsonSerializer.Deserialize<SpeedEvent>(s.Value!)).ToArray();
        }

        private string GetFullKey(Guid siteId)
        {
            return siteId.ToString().ToUpper() + ":SpeedStatus";
        }

        public async Task PutStatusAsync(Guid tenantId, Guid deviceId, SpeedEvent speedEvent, CancellationToken cancellationToken = default)
        {
            await _redisDb.HashSetAsync(GetFullKey(_siteId), new[] { new HashEntry(deviceId.ToString(), JsonSerializer.Serialize(speedEvent))}, CommandFlags.FireAndForget);
            await _redisDb.KeyExpireAsync(GetFullKey(_siteId),TimeSpan.FromSeconds(3600));
        }
    }
}
