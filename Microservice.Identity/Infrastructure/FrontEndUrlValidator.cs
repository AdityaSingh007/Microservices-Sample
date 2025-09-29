using Duende.Bff;

namespace Microservice.Identity.Infrastructure
{
    public class FrontEndUrlValidator : IReturnUrlValidator
    {
        private readonly IConfiguration _configuration;
        public FrontEndUrlValidator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<bool> IsValidAsync(string returnUrl)
        {
            var uri = new Uri(returnUrl);
            return Task.FromResult((uri.Host == (_configuration["FrontEndHost"] ?? String.Empty)) || (uri.Port == Convert.ToInt32(_configuration["FrontEndHost"])));
        }
    }
}
