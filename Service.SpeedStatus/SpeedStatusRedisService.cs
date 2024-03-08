using Econolite.Ode.Status.Speed;
using Econolite.Ode.Repository.SpeedStatus;
using Microsoft.Extensions.Logging;

namespace Econolite.Ode.Service.SpeedStatus
{
    public class SpeedStatusRedisService : ISpeedStatusRedisService
    {
        private readonly ISpeedStatusRepository _speedStatusRepository;
        private readonly ILogger<SpeedEvent> _logger;

        public SpeedStatusRedisService(ISpeedStatusRepository speedStatusRepository, ILogger<SpeedEvent> logger)
        {
            _speedStatusRepository = speedStatusRepository;
            _logger = logger;
        }

        public async Task<List<SpeedEvent?>> GetAllAsync()
        {
            var speedEvents = await _speedStatusRepository.GetAllAsync();

            return speedEvents.ToList() ?? new List<SpeedEvent?>();
        }

        public async Task<SpeedEvent?> GetAsync(Guid id)
        {
            var speedEvent = await _speedStatusRepository.GetAsync(id);
            return speedEvent ?? new SpeedEvent();
        }

        public async Task PublishAsync(Guid tenantId, SpeedEvent? speedEvent)
        {
            if (speedEvent == null)
            {
                throw new ArgumentNullException(nameof(speedEvent));
            }

            try
            {
                var existingEvent = await _speedStatusRepository.GetAsync(speedEvent.DeviceId);
                if (existingEvent == null)
                {
                    await _speedStatusRepository.PutStatusAsync(tenantId, speedEvent.DeviceId, speedEvent);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw;
            }
        }
    }
}
