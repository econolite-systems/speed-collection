using MongoDB.Bson.Serialization.Attributes;

namespace AcyclicaConfigRepository.Models.Acyclica
{
    /// <summary>
    /// AcyclicaConfigDoc
    /// </summary>
    public class AcyclicaConfigDoc
    {
        /// <summary>
        /// Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Url
        /// </summary>
        [BsonElement("Url")]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// ApiKey
        /// </summary>
        [BsonElement("ApiKey")]
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// PollInterval
        /// </summary>
        [BsonElement("PollInterval")]
        public int PollInterval { get; set; }
    }
}
