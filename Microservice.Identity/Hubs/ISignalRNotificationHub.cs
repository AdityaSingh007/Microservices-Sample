namespace Microservice.Identity.Hubs
{
    public interface ISignalRNotificationClient
    {
        Task SendNotification(string message, dynamic payLoad);
    }
}
