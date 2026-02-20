using Azure.API.Config;
using Azure.API.Models.Profiles;
using Azure.API.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Azure.API.Services.Interfaces;
using System.Net;

namespace Azure.API.Services;

public class ProfileService : IProfileService
{
    private readonly HttpClient httpClient;
    private readonly WorkItemUri wi;
    private readonly ILogger<ProfileService> logger;

    public ProfileService(IHttpClientFactory httpClientFactory, IOptions<WorkItemUri> wi, ILogger<ProfileService> logger)
    {
        this.httpClient = httpClientFactory.CreateClient("AzureDevOps");
        this.wi = wi.Value;
        this.logger = logger;
    }

    public async Task<ProfileList> GetUserProfilesAsync(CancellationToken continuationToken = default)
    {
        string[] wis = WorkItemUtils.WorkItemConnectionParameters(this.wi);

        string url = $"https://vsaex.dev.azure.com/{wis[0]}/_apis/userentitlements?api-version=7.2-preview.5";

        if (this.httpClient.DefaultRequestHeaders.Authorization == null)
        {
            this.logger.LogWarning("HttpClient has no Authorization header configured. Ensure PAT is set in configuration and the named client 'AzureDevOps' is configured in Program.cs.");
        }

        using HttpResponseMessage response = await this.httpClient.GetAsync(url, continuationToken);

        await EnsureSuccessOrThrowAsync(response, continuationToken);

        var content = await response.Content.ReadAsStringAsync(continuationToken);

        try
        {
            var profiles = ParseProfilesFromJson(content);
            return new ProfileList { Profiles = profiles };
        }
        catch (JsonException ex)
        {
            this.logger.LogError(ex, "Failed to parse user profiles JSON.");
            throw;
        }
    }

    private async Task EnsureSuccessOrThrowAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode) return;

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            this.logger.LogError("Failed to retrieve user profiles. Status code: Unauthorized (401). Possible causes: invalid/expired PAT or missing PAT scopes for reading user entitlements.");
            var errContent = await response.Content.ReadAsStringAsync(ct);
            this.logger.LogDebug("Unauthorized response content: {Content}", errContent);
            throw new HttpRequestException("Request unauthorized (401). Check PAT and its scopes.");
        }

        this.logger.LogError("Failed to retrieve user profiles. Status code: {StatusCode}", response.StatusCode);
        throw new HttpRequestException($"Request failed with status code: {response.StatusCode}");
    }

    private List<Profile> ParseProfilesFromJson(string content)
    {
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        JsonElement itemsArray;
        if (!TryGetItemsArray(root, out itemsArray))
        {
            return new List<Profile>();
        }

        var profiles = new List<Profile>();
        foreach (var item in itemsArray.EnumerateArray())
        {
            var p = ExtractProfileFromElement(item);
            if (p != null)
                profiles.Add(p);
        }

        return profiles;
    }

    private static bool TryGetItemsArray(JsonElement root, out JsonElement itemsArray)
    {
        itemsArray = default;
        if (root.ValueKind != JsonValueKind.Object) return false;

        if (root.TryGetProperty("items", out itemsArray) && itemsArray.ValueKind == JsonValueKind.Array)
            return true;

        if (root.TryGetProperty("value", out itemsArray) && itemsArray.ValueKind == JsonValueKind.Array)
            return true;

        return false;
    }

    private Profile? ExtractProfileFromElement(JsonElement item)
    {
        if (item.ValueKind != JsonValueKind.Object) return null;

        string id = string.Empty;
        string displayName = string.Empty;
        string email = string.Empty;

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
        if (string.IsNullOrEmpty(displayName) && item.TryGetProperty("displayName", out var d2) && d2.ValueKind == JsonValueKind.String)
            displayName = d2.GetString() ?? string.Empty;

        if (string.IsNullOrEmpty(email))
        {
            if (item.TryGetProperty("mailAddress", out var m2) && m2.ValueKind == JsonValueKind.String)
                email = m2.GetString() ?? string.Empty;

            if (string.IsNullOrEmpty(email) && item.TryGetProperty("principalName", out var p1) && p1.ValueKind == JsonValueKind.String)
                email = p1.GetString() ?? string.Empty;
        }

        if (string.IsNullOrEmpty(displayName) && string.IsNullOrEmpty(email)) return null;

        return new Profile { Id = id, DisplayName = displayName, Email = email };
    }
}