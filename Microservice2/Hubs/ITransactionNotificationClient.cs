namespace Microservice2.Hubs
{
    public interface ITransactionNotificationClient
    {
        Task NotifyTransactionStatus(string message);
    }
}
