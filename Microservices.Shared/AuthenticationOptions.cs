namespace Microservices.Shared
{
    public class AuthenticationOptions
    {
        public const string ConfigurationSection = "AuthenticationParameters";
        public string Authority { get; set; } = string.Empty;
        public string[] Audience { get; set; } = [];
        public string Scope { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string TokenEndpoint { get; set; } = string.Empty;
        public string TokenExchangeAudience { get; set; } = string.Empty;
        public string TokenExchangeScope { get; set; } = string.Empty;
        public bool RequireHttpsMetadata { get; set; } = false;
        public bool ValidateAudience { get; set; } = true;
        public List<string> RequiredRoles { get; set; } = [];
    }
}
