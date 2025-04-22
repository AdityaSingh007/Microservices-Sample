namespace Microservices.Common.Models
{
    public class LoggingOptions
    {
        public const string ConfigurationSection = "LoggingParameters";
        public string ElasticUserName { get; set; } = string.Empty;
        public string ElasticPassword { get; set; } = string.Empty;
        public string LoggingUrl { get; set; } = string.Empty;
    }
}
