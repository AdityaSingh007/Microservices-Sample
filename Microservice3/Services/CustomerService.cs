using Grpc.Core;
using Microservice3.Contracts;
using Microservice3.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Microservice3.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CustomerService> logger;
        private readonly IHttpContextAccessor httpContextAccessor;

        public CustomerService(HttpClient httpClient,
            IConfiguration configuration,
            ILogger<CustomerService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            this._httpClient = httpClient;
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
            _httpClient.BaseAddress = new Uri(configuration["ApiUrls:CustomerApiUrl"] ?? string.Empty);
        }

        public async Task<CustomerDto> GetCustomerDetails(Guid customerId)
        {
            var httpContext = httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "HttpContext is null."));
            }

            var access_Token = await httpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, "access_token");
            if (string.IsNullOrWhiteSpace(access_Token))
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Access token is null or empty."));
            }

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", access_Token);

            var httpResponseMessage = await _httpClient.GetAsync($"api/v1/Customer/{customerId}");

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    logger.LogError("Customer not found for id - {@customerId}.", customerId.ToString());
                    throw new RpcException(new Status(StatusCode.NotFound, "Customer not found."));
                }
                else
                {
                    logger.LogError("Cannot get customer details - Reason:{@reason}", await httpResponseMessage.Content.ReadAsStringAsync());
                    throw new RpcException(new Status((Grpc.Core.StatusCode)httpResponseMessage.StatusCode, httpResponseMessage.ReasonPhrase ?? string.Empty));
                }
            }

            return await httpResponseMessage.Content.ReadFromJsonAsync<CustomerDto?>() ?? throw new RpcException(new Status(StatusCode.Internal, "Error occured while parsing get customer details response.")); ;
        }

    }
}
