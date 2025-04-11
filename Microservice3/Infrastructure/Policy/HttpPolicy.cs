using Polly;
using Polly.Extensions.Http;

namespace Microservice3.Infrastructure.Policy
{
    public static class HttpPolicy
    {
        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions.HandleTransientHttpError()
                .WaitAndRetryAsync(5,
                retyAttempt => TimeSpan.FromMilliseconds(Math.Pow(1.5, retyAttempt) * 1000),
                (_, waitingTime) =>
                {
                    Console.WriteLine("Retry due to polly retry error");
                }
                );
        }

        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy() 
        {
            return HttpPolicyExtensions.HandleTransientHttpError()
                                       .CircuitBreakerAsync(3, TimeSpan.FromSeconds(15));
        }
    }
}
