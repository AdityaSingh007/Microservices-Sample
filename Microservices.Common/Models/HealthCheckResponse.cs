namespace Microservices.Common.Models
{
    public class HealthCheckResponse
    {
        public string Status { get; set; } = string.Empty;
        public string[] Checks { get; set; } = [];
    }
}
