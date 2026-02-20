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
    private readonly AzureDevOpsOptions azureDevOpsOptions;
    private readonly ILogger<ProfileService> logger;

    public ProfileService(IHttpClientFactory httpClientFactory, IOptions<AzureDevOpsOptions> azureDevOpsOptions, ILogger<ProfileService> logger)
    {
        this.httpClient = httpClientFactory.CreateClient("AzureDevOps");
        this.azureDevOpsOptions = azureDevOpsOptions.Value;
        this.logger = logger;
    }

    public async Task<ProfileList> GetUserProfilesAsync(CancellationToken continuationToken = default)
    {
        string url = $"{this.azureDevOpsOptions.EntitlementsBaseUrl}/{this.azureDevOpsOptions.Organization}/_apis/userentitlements?api-version=7.2-preview.5";

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
            this.logger.LogError(ex, "Failed to parse user profiles JSON. Content: {Content}", content);
            throw new JsonException($"Failed to parse user profiles JSON. See inner exception for details. Content: {content}", ex);
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

    private static List<Profile> ParseProfilesFromJson(string content)
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

    private static Profile? ExtractProfileFromElement(JsonElement item)
    {
        if (item.ValueKind != JsonValueKind.Object) return null;

        string id = TryGetString(item, "id");
        string displayName = string.Empty;
        string email = string.Empty;

        if (item.TryGetProperty("user", out var userProp) && userProp.ValueKind == JsonValueKind.Object)
        {
            displayName = TryGetString(userProp, "displayName");
            email = TryGetString(userProp, "mailAddress");
            if (string.IsNullOrEmpty(id))
                id = TryGetString(userProp, "id");
            if (string.IsNullOrEmpty(displayName))
                displayName = TryGetString(userProp, "uniqueName");
        }

        if (string.IsNullOrEmpty(displayName))
            displayName = TryGetString(item, "displayName");

        if (string.IsNullOrEmpty(email))
        {
            email = TryGetString(item, "mailAddress");
            if (string.IsNullOrEmpty(email))
                email = TryGetString(item, "principalName");
        }

        if (string.IsNullOrEmpty(displayName) && string.IsNullOrEmpty(email)) return null;

        return new Profile { Id = id, DisplayName = displayName, Email = email };
    }

    private static string TryGetString(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.String)
            return prop.GetString() ?? string.Empty;
        return string.Empty;
    }
}