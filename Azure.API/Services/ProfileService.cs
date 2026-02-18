using Azure.API.Client;

namespace Azure.API.Services;

public class ProfileService
{
    private AzureHttpClient httpClient;

    public ProfileService(AzureHttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task GetUserProfilesAsync(string select, string filter, string orderBy, CancellationToken continuationToken = default)
    {
        string[] wis = this.httpClient.WorkItemConnectionParameters();

        string url = $"https://vsaex.dev.azure.com/{wis[0]}/_apis/userentitlements?api-version=7.2-preview.5";

        using HttpResponseMessage response = await this.httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            
        }
    }
}