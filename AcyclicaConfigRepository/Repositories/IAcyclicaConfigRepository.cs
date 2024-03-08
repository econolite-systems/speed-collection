using AcyclicaConfigRepository.Models.Acyclica;

namespace AcyclicaConfigRepository.Repositories
{
    public interface IAcyclicaConfigRepository
    {
        /// <summary>
        /// FindOneAsync
        /// </summary>
        /// <returns></returns>
        Task<AcyclicaConfigDto?> FindOneAsync();

        /// <summary>
        /// InsertOneAsync
        /// </summary>
        /// <param name="acyclicaConfigDto"></param>
        /// <returns></returns>
        Task InsertOneAsync(AcyclicaConfigDto acyclicaConfigDto);

        /// <summary>
        /// FindOneAndReplaceAsync
        /// </summary>
        /// <param name="acyclicaConfigDto"></param>
        /// <returns></returns>
        Task FindOneAndReplaceAsync(AcyclicaConfigDto acyclicaConfigDto);

        /// <summary>
        /// FindOneAndDeleteAsync
        /// </summary>
        /// <param name="acyclicaConfigDto"></param>
        /// <returns></returns>
        Task FindOneAndDeleteAsync(AcyclicaConfigDto acyclicaConfigDto);
    }
}