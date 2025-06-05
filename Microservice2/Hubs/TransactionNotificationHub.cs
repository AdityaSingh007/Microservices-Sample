using Microsoft.AspNetCore.SignalR;

namespace Microservice2.Hubs
{
    public class TransactionNotificationHub : Hub<ITransactionNotificationClient>
    {

    }
}
