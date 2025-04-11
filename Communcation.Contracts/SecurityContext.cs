namespace Communcation.Contracts
{
    public class SecurityContext
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public SecurityContext(string accessToken)
        {
            AccessToken = accessToken;
        }
        public string AccessToken { get; set; } = string.Empty;
    }
}
