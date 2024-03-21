class Program
{
    static async Task Main(string[] args)
    {
        var clientId = "your-client-id";
        var clientSecret = "your-client-secret";
        var tokenEndpoint = "https://login.microsoftonline.com/your-tenant-id/oauth2/token";
        var resource = "https://api.businesscentral.dynamics.com/v1.0/your-environment/api/v1.0/";

        using (var apiClient = new BusinessCentralApiClient(tokenEndpoint, resource, clientId, clientSecret))
        {
            Brapi.ErrorLog.LogMessage(si,"Connection Succesfull")
        }
    }
}