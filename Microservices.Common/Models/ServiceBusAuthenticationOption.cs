namespace Microservices.Common.Models
{
    public class ServiceBusAuthenticationOption
    {
        public const string ConfigurationSection = "ServiceBusAuthenticationParameters";
        public string ServiceBus_ClientId { get; set; } = string.Empty;
        public string ServiceBus_ClientSecret { get; set; } = string.Empty;
        public string ServiceBus_Scope { get; set; } = string.Empty;
        public string ServiceBus_Audience { get; set; } = string.Empty;
        public string Authority { get; set; } = string.Empty;
        public string TokenEndpoint { get; set; } = string.Empty;
    }
}
