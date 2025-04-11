using AutoMapper;
using Communcation.Contracts;
using Duende.AccessTokenManagement;
using Grpc.Core;
using MassTransit;
using Microservice3.Contracts;
using Microservice3.Domain.Entities;
using Microservice3.Infrastructure.Repositories;
using Microservices.Common;
using Microservices.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Microservice3.Services
{
    [Authorize(AuthorizationConstants.BearerPolicyName)]
    [Authorize(AuthorizationConstants.ValidateScopePolicyName)]
    [Authorize(AuthorizationConstants.ValidateAudiencesPolicyName)]
    [Authorize(AuthorizationConstants.ValidateRolePolicyName)]
    public class CustomerAccountService : CustomerAccount.CustomerAccountBase
    {
        private readonly ILogger<CustomerAccountService> logger;
        private readonly IAccountRepository accountRepository;
        private readonly ICustomerService customerService;
        private readonly IMapper mapper;
        private readonly IPublishEndpoint publishEndpoint;
        private readonly IClientCredentialsTokenManagementService clientCredentialsTokenManagementService;
        private readonly IOptions<ServiceBusAuthenticationOption> options;

        public CustomerAccountService(ILogger<CustomerAccountService> logger,
            IAccountRepository accountRepository,
            ICustomerService customerService,
            IMapper mapper,
            IPublishEndpoint publishEndpoint,
            IClientCredentialsTokenManagementService clientCredentialsTokenManagementService,
            IOptions<ServiceBusAuthenticationOption> options)
        {
            this.logger = logger;
            this.accountRepository = accountRepository;
            this.customerService = customerService;
            this.mapper = mapper;
            this.publishEndpoint = publishEndpoint;
            this.clientCredentialsTokenManagementService = clientCredentialsTokenManagementService;
            this.options = options;
        }

        public override async Task<GetAccountResponse> GetById(GetAccountRequest request, ServerCallContext context)
        {
            if (!Guid.TryParse(request.AccountId, out Guid accountId))
            {
                logger.LogError("Invalid account Id.");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid account Id."));
            }

            var account = await accountRepository.GetByIdAsync(accountId);
            if (account == null)
            {
                logger.LogError("Account not found for id - {@accountId}.", accountId.ToString());
                throw new RpcException(new Status(StatusCode.NotFound, "Account not found."));
            }

            return mapper.Map<GetAccountResponse>(account);
        }

        public override async Task<CreateAccountResponse> CreateAccount(CreateAccountRequest request, ServerCallContext context)
        {
            using var scope = logger.BeginScope("Creating account for customer with id - {CustomerId}", request.CustomerId);

            if (!Guid.TryParse(request.CustomerId, out Guid customerId))
            {
                logger.LogError("Invalid customer Id.");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid customer Id."));
            }

            var customer = await customerService.GetCustomerDetails(customerId);

            if (customer == null)
            {
                logger.LogError("Customer not found for id - {@customerId}.", customerId.ToString());
                throw new RpcException(new Status(StatusCode.NotFound, "Customer not found."));
            }

            var result = await accountRepository.AddAsync(mapper.Map<Account>(request));
            return mapper.Map<CreateAccountResponse>(result);
        }

        public override async Task<AddResponse> AddToAccount(AddRequest request, ServerCallContext context)
        {
            using var scope = logger.BeginScope("Processing add to account request for request - {traceId}",
                Activity.Current != null ? Guid.Parse(Activity.Current.TraceId.ToString()) : Guid.NewGuid());

            try
            {
                if (!Guid.TryParse(request.AccountId, out Guid accountId))
                {
                    logger.LogError("Invalid account Id.");
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid account Id."));
                }

                var account = await accountRepository.GetByIdAsync(accountId);

                if (account == null)
                {
                    logger.LogError("Account not found for id - {@accountId}.", accountId.ToString());
                    throw new RpcException(new Status(StatusCode.NotFound, "Account not found."));
                }

                account.Balance += Convert.ToDecimal(request.Amount);

                await accountRepository.UpdateAsync(account);

                var serviceBus_AccessToken = await clientCredentialsTokenManagementService.GetAccessTokenAsync(options.Value.ServiceBus_ClientId);

                await publishEndpoint.Publish(new AccountTransactionEvent(Activity.Current != null ? Guid.Parse(Activity.Current.TraceId.ToString()) : Guid.NewGuid(), DateTime.UtcNow)
                {
                    CustomerId = account.CustomerId,
                    AccountId = account.Id,
                    Amount = Convert.ToDecimal(request.Amount),
                    Type = TransactionType.Adding,
                    SecurityContext = new SecurityContext(serviceBus_AccessToken.AccessToken ?? string.Empty)
                });
                logger.LogInformation("Add to account successfull for account id - {accountId}", accountId.ToString());
                return mapper.Map<AddResponse>(account);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while adding to account.");
                throw new RpcException(new Status(StatusCode.Internal, "Error occurred while adding to account."));
            }
        }

        public override async Task<WithdrawResponse> Withdrawing(WithdrawRequest request, ServerCallContext context)
        {
            using var scope = logger.BeginScope("Processing withdraw from account request for request - {traceId}",
                Activity.Current != null ? Guid.Parse(Activity.Current.TraceId.ToString()) : Guid.NewGuid());

            try
            {
                if (!Guid.TryParse(request.AccountId, out Guid accountId))
                {
                    logger.LogError("Invalid account Id.");
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid account Id."));
                }

                var account = await accountRepository.GetByIdAsync(accountId);

                if (account == null)
                {
                    logger.LogError("Account not found for id - {@accountId}.", accountId.ToString());
                    throw new RpcException(new Status(StatusCode.NotFound, "Account not found."));
                }

                if (account.Balance <= 0)
                {
                    logger.LogError("Account balance is zero for account id - {@accountId}.", accountId.ToString());
                    throw new RpcException(new Status(StatusCode.Internal, "Account balance is zero."));
                }

                account.Balance -= Convert.ToDecimal(request.Amount);

                await accountRepository.UpdateAsync(account);

                var serviceBus_AccessToken = await clientCredentialsTokenManagementService.GetAccessTokenAsync(options.Value.ServiceBus_ClientId);

                await publishEndpoint.Publish(new AccountTransactionEvent(Activity.Current != null ? Guid.Parse(Activity.Current.TraceId.ToString()) : Guid.NewGuid(), DateTime.UtcNow)
                {
                    CustomerId = account.CustomerId,
                    AccountId = account.Id,
                    Amount = Convert.ToDecimal(request.Amount),
                    Type = TransactionType.Withdrawing,
                    SecurityContext = new SecurityContext(serviceBus_AccessToken.AccessToken ?? string.Empty)
                });
                logger.LogInformation("Withdraw from account successfull for account id - {accountId}", accountId.ToString());
                return mapper.Map<WithdrawResponse>(account);
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while withdrawing from account.");
                throw new RpcException(new Status(StatusCode.Internal, "Error occurred while withdrawing from account."));
            }
        }
    }
}
