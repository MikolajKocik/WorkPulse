using Azure.API.Config;
using Azure.API.Models.Profiles;
using Azure.API.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Azure.API.Services;

public class ProfileService
{
    private HttpClient httpClient;
    private WorkItemURI wi;
    private ILogger<ProfileService> logger;

    public ProfileService(IHttpClientFactory httpClientFactory, IOptions<WorkItemURI> wi, ILogger<ProfileService> logger)
    {
        this.httpClient = httpClientFactory.CreateClient("AzureDevOps");
        this.wi = wi.Value;
        this.logger = logger;
    }

    public async Task<ProfileList> GetUserProfilesAsync(CancellationToken continuationToken = default)
    {
        string[] wis = WorkItemUtils.WorkItemConnectionParameters(this.wi);

        string url = $"https://vsaex.dev.azure.com/{wis[0]}/_apis/userentitlements?api-version=7.2-preview.5";

        using HttpResponseMessage response = await this.httpClient.GetAsync(url, continuationToken);

        if (!response.IsSuccessStatusCode)
        {
            this.logger.LogError("Failed to retrieve user profiles. Status code: {StatusCode}", response.StatusCode);
            throw new HttpRequestException($"Request failed with status code: {response.StatusCode}");
        }

        string content = await response.Content.ReadAsStringAsync(continuationToken);

        try
        {
            using var doc = JsonDocument.Parse(content);
            JsonElement root = doc.RootElement;

            var profiles = new List<Profile>();

            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("value", out var value) && value.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in value.EnumerateArray())
                {
                    string id = string.Empty;
                    string displayName = string.Empty;
                    string email = string.Empty;

                    // Common shapes: item.user.displayName / item.user.mailAddress / item.id
                    if (item.ValueKind == JsonValueKind.Object)
                    {
                        if (item.TryGetProperty("id", out var idProp) && idProp.ValueKind == JsonValueKind.String)
                            id = idProp.GetString() ?? string.Empty;

                        if (item.TryGetProperty("user", out var userProp) && userProp.ValueKind == JsonValueKind.Object)
                        {
                            if (userProp.TryGetProperty("displayName", out var d1) && d1.ValueKind == JsonValueKind.String)
                                displayName = d1.GetString() ?? string.Empty;

                            if (userProp.TryGetProperty("mailAddress", out var m1) && m1.ValueKind == JsonValueKind.String)
                                email = m1.GetString() ?? string.Empty;

                            if (userProp.TryGetProperty("id", out var uid) && string.IsNullOrEmpty(id) && uid.ValueKind == JsonValueKind.String)
                                id = uid.GetString() ?? string.Empty;

                            if (string.IsNullOrEmpty(displayName) && userProp.TryGetProperty("uniqueName", out var un) && un.ValueKind == JsonValueKind.String)
                                displayName = un.GetString() ?? string.Empty;
                        }

                        // Fallbacks
                        if (string.IsNullOrEmpty(displayName))
                        {
                            if (item.TryGetProperty("displayName", out var d2) && d2.ValueKind == JsonValueKind.String)
                                displayName = d2.GetString() ?? string.Empty;
                        }

                        if (string.IsNullOrEmpty(email))
                        {
                            if (item.TryGetProperty("mailAddress", out var m2) && m2.ValueKind == JsonValueKind.String)
                                email = m2.GetString() ?? string.Empty;

                            if (string.IsNullOrEmpty(email) && item.TryGetProperty("principalName", out var p1) && p1.ValueKind == JsonValueKind.String)
                                email = p1.GetString() ?? string.Empty;
                        }
                    }

                    if (!string.IsNullOrEmpty(displayName) || !string.IsNullOrEmpty(email))
                    {
                        profiles.Add(new Profile
                        {
                            Id = id,
                            DisplayName = displayName,
                            Email = email
                        });
                    }
                }
            }

            return new ProfileList { Profiles = profiles };
        }
        catch (JsonException ex)
        {
            this.logger.LogError(ex, "Failed to parse user profiles JSON.");
            throw;
        }
    }
}