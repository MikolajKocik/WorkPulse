using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Azure.API.Config;
using Azure.API.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Azure.API.Models.WorkItems.Requests;
using Azure.API.Models.WorkItems.Responses;

namespace Azure.API.Services;

public sealed class WorkItemService
{
    private readonly WorkItemURI wi;
    private readonly HttpClient httpClient;
    private readonly ILogger<WorkItemService> logger;

    public WorkItemService(
        IOptions<WorkItemURI> wi,
        IHttpClientFactory httpClientFactory,
        ILogger<WorkItemService> logger
        )
    {
        this.wi = wi.Value;
        this.httpClient = httpClientFactory.CreateClient("AzureDevOps");
        this.logger = logger;
    }

    public async Task<WorkItemListResponse> GetWorkItemListAsync(int[] ids, string[]? fields = null, CancellationToken ct = default)
    {
        if (ids == null || ids.Length == 0)
        {
            throw new ArgumentException("IDs cannot be null or empty.", nameof(ids));
        }

        if (ids.Length > 200)
        {
            throw new ArgumentException("Maximum 200 IDs allowed.", nameof(ids));
        }

        string[] wis = WorkItemUtils.WorkItemConnectionParameters(this.wi);
        string url = $"https://dev.azure.com/{wis[0]}/{wis[1]}/_apis/wit/workitemsbatch?api-version=7.1";

        var requestBody = new WorkItemBatchRequest
        {
            Ids = ids,
            Fields = fields ?? ["System.Id", "System.Title", "System.WorkItemType"]
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        using var content = new StringContent(JsonSerializer.Serialize(requestBody, jsonOptions), Encoding.UTF8, "application/json");
        using HttpResponseMessage response = await this.httpClient.PostAsync(url, content, ct);

        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync(ct);
            this.logger.LogInformation("Response content: {Content}", responseContent);
            if (responseContent.StartsWith("<"))
            {
                throw new Exception("Received HTML instead of JSON. Check API credentials or URL.");
            }
            var result = await response.Content.ReadFromJsonAsync<WorkItemListResponse>(jsonOptions, ct);
            this.logger.LogInformation("Successfully retrieved {Count} work items.", result?.Count ?? 0);
            return result!;
        }
        else
        {
            string errorMessage = await response.Content.ReadAsStringAsync(ct);
            this.logger.LogError("Failed to get work items. Status: {Status}, Error: {Error}", response.StatusCode, errorMessage);
            throw new HttpRequestException($"Failed to get work items: {response.ReasonPhrase}");
        }
    }

    public async Task<WorkItemResponse> GetWorkItemInfoAsync(int id)
    {
        string[] wis = WorkItemUtils.WorkItemConnectionParameters(this.wi);

        string url = $"https://dev.azure.com/{wis[0]}/{wis[1]}/_apis/wit/workitems/{id}?api-version=7.1";
        using HttpResponseMessage response = await this.httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<WorkItemResponse>();
            return result!;
        }
        else 
        {
            HttpStatusCode status = response.StatusCode;
            string? reason = response.ReasonPhrase;

            logger.LogError("Error occured while loading Work Item: {StatusCode} - {Reason}", status, reason);
            throw new HttpRequestException($"API Error: {status} - {reason}");
        }
    }

    public async Task<string> CreateWorkItemAsync(WorkItemRequest request, string? type = null, CancellationToken ct = default)
    {
        string[] wis = WorkItemUtils.WorkItemConnectionParameters(this.wi);

        if (type != null)
        {
            if (!this.wi.SupportedTypes.Contains(type) || string.IsNullOrWhiteSpace(type))
            {
                type = this.wi.DefaultType;
            }
        }
        else
        {
            type = this.wi.DefaultType;
        }

        string url = $"https://dev.azure.com/{wis[0]}/{wis[1]}/_apis/wit/workitems/${type}?api-version=7.1";

        List<object> operations = WorkItemUtils.RequestOperations(request);

        string json = JsonSerializer.Serialize(operations);
        var content = new StringContent(json , Encoding.UTF8, "application/json-patch+json");

        using HttpResponseMessage response = await this.httpClient.PostAsync(url, content, ct);
        
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            return result;
        }
        else 
        {
            HttpStatusCode status = response.StatusCode;
            string? reason = response.ReasonPhrase;

            logger.LogError("Error during creating the Work Item: {StatusCode} - {Reason}", status, reason);
            throw new HttpRequestException($"API Error: {status} - {reason}");
        }
    }

    public async Task<bool> DeleteWorkItemAsync(int id)
    {

        string[] wis = WorkItemUtils.WorkItemConnectionParameters(this.wi);

        string url = $"https://dev.azure.com/{wis[0]}/{wis[1]}/_apis/wit/workitems/{id}?api-version=7.1";

        using HttpResponseMessage response = await this.httpClient.DeleteAsync(url);

         if (response.IsSuccessStatusCode)
        {
            return true;
        }
        else 
        {
            HttpStatusCode status = response.StatusCode;
            string? reason = response.ReasonPhrase;

            logger.LogError("Error during deleting Work Item: {StatusCode} - {Reason}", status, reason);
            throw new HttpRequestException($"API Error: {status} - {reason}");
        }
    }

    public async Task<string> UpdateWorkItemAsync(int id, WorkItemRequest request, CancellationToken ct = default)
    {
        string[] wis = WorkItemUtils.WorkItemConnectionParameters(this.wi);

        string url = $"https://dev.azure.com/{wis[0]}/{wis[1]}/_apis/wit/workitems/{id}?api-version={wis[2]}";

        List<object> operations = WorkItemUtils.RequestOperations(request);

        string json = JsonSerializer.Serialize(operations);
        var content = new StringContent(json , Encoding.UTF8, "application/json-patch+json");

        using HttpResponseMessage response = await this.httpClient.PatchAsync(url, content, ct);

         if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            return result;
        }
        else 
        {
            HttpStatusCode status = response.StatusCode;
            string? reason = response.ReasonPhrase;

            logger.LogError("Error during updating the Work Item: {StatusCode} - {Reason}", status, reason);
            throw new HttpRequestException($"API Error: {status} - {reason}");
        }
    }
}