using AutoMapper;
using Communcation.Contracts;
using MassTransit;
using Microservice3.Domain.Entities;
using Microservice3.Infrastructure.Repositories.Interface;
using Microservices.Common.Authorization;
using Microservices.Common.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Microservice3.EventBusConsumer
{
    public class AccountTransactionConsumer : IConsumer<AccountTransactionEvent>
    {
        private readonly ILogger<AccountTransactionConsumer> _logger;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IMapper _mapper;
        private readonly ICustomTokenValidation customTokenValidation;
        private readonly IOptions<ServiceBusAuthenticationOption> options;

        public AccountTransactionConsumer(ILogger<AccountTransactionConsumer> logger,
            ITransactionRepository transactionRepository,
            IMapper mapper,
            ICustomTokenValidation customTokenValidation,
            IOptions<ServiceBusAuthenticationOption> options)
        {
            _logger = logger;
            _transactionRepository = transactionRepository;
            _mapper = mapper;
            this.customTokenValidation = customTokenValidation;
            this.options = options;
        }

        public async Task Consume(ConsumeContext<AccountTransactionEvent> context)
        {
            using var scope = _logger.BeginScope("Starting processing transaction for request - {traceId}", context.Message.CorrelationId);
            try
            {
                var accessToken = context.Message.SecurityContext?.AccessToken;
                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    _logger.LogError("Access token is null or empty");
                    return;
                }
                var tokenValidationResult = await customTokenValidation.ValidateToken(accessToken, new TokenValidationParameters() 
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidIssuer = options.Value.Authority,
                    ValidAudience = options.Value.ServiceBus_Audience
                });
                if (!tokenValidationResult)
                {
                    _logger.LogError("Token validation failed");
                    return;
                }
                var transaction = _mapper.Map<Transaction>(context.Message);
                if (transaction is not null)
                {
                    await _transactionRepository.Add(transaction);
                    _logger.LogInformation("Processed transaction with id - {@transactionId}", transaction.Id);
                }
                else
                    throw new ArgumentNullException(nameof(transaction));
            }
            catch (Exception ex)
            {
                _logger.LogError("Cannot process transaction : Reason - {@reason}", ex.Message.ToString());
            }
        }
    }
}
