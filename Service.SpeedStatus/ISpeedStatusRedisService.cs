using Econolite.Ode.Status.Speed;

namespace Econolite.Ode.Service.SpeedStatus
{
    public interface ISpeedStatusRedisService
    {
        Task<List<SpeedEvent?>> GetAllAsync();
        Task<SpeedEvent?> GetAsync(Guid id);
        Task PublishAsync(Guid tenantId, SpeedEvent? speedEvent);
    }
}
