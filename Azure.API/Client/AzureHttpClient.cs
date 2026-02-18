namespace Azure.API.Client;

public sealed class AzureHttpClient
{
    private readonly HttpClient httpClient;
    
    public AzureHttpClient(IHttpClientFactory httpClientFactory)
    {
        this.httpClient = httpClientFactory.CreateClient("AzureDevOps");
    }

    private string[] WorkItemConnectionParameters()
    {
        string organization = this.wi.Organization;
        string project = this.wi.Project;

        return [organization, project];
    }
}
