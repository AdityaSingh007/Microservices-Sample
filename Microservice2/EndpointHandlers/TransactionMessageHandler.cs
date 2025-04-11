using AutoMapper;
using Microservice3.Domain.Entities;
using Microservice3.Infrastructure.Repositories.Interface;
using Microservice3.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Microservice3.EndpointHandlers
{
    public static class TransactionMessageHandler
    {
        public static async Task<Results<NotFound, Ok<List<TransactionDto>>>> GetByAccountId(
            ITransactionRepository transactionRepository,
            IMapper mapper,
            Guid accountId,
            ILogger<Transaction> logger)
        {
            var transaction = await transactionRepository.GetByAccountId(accountId);

            if (transaction == null) 
            {
                logger.LogWarning("No transactions found for account {@accountId}", accountId);
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(mapper.Map<List<TransactionDto>>(transaction.ToList()));
        }
    }
}
