using System.Linq;
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
using Azure.API.Models.WorkItems;
using Azure.API.Services.Interfaces;

namespace Azure.API.Services;

public sealed class WorkItemService : IWorkItemService
{
    private readonly AzureDevOpsOptions azureDevOpsOptions;
    private readonly HttpClient httpClient;
    private readonly ILogger<WorkItemService> logger;
    private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public WorkItemService(
        IOptions<AzureDevOpsOptions> azureDevOpsOptions,
        IHttpClientFactory httpClientFactory,
        ILogger<WorkItemService> logger
        )
    {
        this.azureDevOpsOptions = azureDevOpsOptions.Value;
        this.httpClient = httpClientFactory.CreateClient("AzureDevOps");
        this.logger = logger;
    }

    /// <summary>
    /// Gets the work item list.
    /// </summary>
    /// <param name="ids">The IDs of the work items.</param>
    /// <param name="fields">The fields to include.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The work item list.</returns>
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

        string url = $"{this.azureDevOpsOptions.BaseUrl}/{this.azureDevOpsOptions.Organization}/{this.azureDevOpsOptions.Project}/_apis/wit/workitemsbatch?api-version=7.1";

        var requestBody = new WorkItemBatchRequest
        {
            Ids = ids,
            Fields = fields ?? ["System.Id", "System.Title", "System.WorkItemType"]
        };

        using var content = new StringContent(JsonSerializer.Serialize(requestBody, jsonOptions), Encoding.UTF8, "application/json");
        using HttpResponseMessage response = await this.httpClient.PostAsync(url, content, ct);

        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync(ct);
            this.logger.LogInformation("Response content: {Content}", responseContent);
            if (responseContent.StartsWith('<'))
            {
                throw new Exception("Received HTML instead of JSON. Check API credentials or URL.");
            }
            var result = await response.Content.ReadFromJsonAsync<WorkItemListResponse>(jsonOptions, ct);
            this.logger.LogInformation("Successfully retrieved {Count} work items.", result?.Count ?? 0);
            return result!;
        }
        else
        {
            await HttpUtils.HandleErrorResponse(response);
            return null!;
        }
    }

    /// <summary>
    /// Gets the work item info.
    /// </summary>
    /// <param name="id">The ID of the work item.</param>
    /// <returns>The work item info.</returns>
    public async Task<WorkItemResponse> GetWorkItemInfoAsync(int id)
    {
        string url = $"{this.azureDevOpsOptions.BaseUrl}/{this.azureDevOpsOptions.Organization}/{this.azureDevOpsOptions.Project}/_apis/wit/workitems/{id}?api-version=7.1";
        using HttpResponseMessage response = await this.httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<WorkItemResponse>(jsonOptions);
            return result!;
        }
        else 
        {
            await HttpUtils.HandleErrorResponse(response);
            return null!;
        }
    }

    /// <summary>
    /// Creates a work item.
    /// </summary>
    /// <param name="request">The work item request.</param>
    /// <param name="type">The type of the work item.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The ID of the created work item.</returns>
    public async Task<string> CreateWorkItemAsync(WorkItemRequest request, string? type = null, CancellationToken ct = default)
    {
        if (type != null)
        {
            if (!this.azureDevOpsOptions.SupportedTypes.Contains(type) || string.IsNullOrWhiteSpace(type))
            {
                type = this.azureDevOpsOptions.DefaultType;
            }
        }
        else
        {
            type = this.azureDevOpsOptions.DefaultType;
        }

        string url = $"{this.azureDevOpsOptions.BaseUrl}/{this.azureDevOpsOptions.Organization}/{this.azureDevOpsOptions.Project}/_apis/wit/workitems/${type}?api-version=7.1";

        List<object> operations = WorkItemUtils.RequestOperations(request);

        string json = JsonSerializer.Serialize(operations);
        var content = new StringContent(json , Encoding.UTF8, "application/json-patch+json");

        using HttpResponseMessage response = await this.httpClient.PostAsync(url, content, ct);
        
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync(ct);
            return result;
        }
        else 
        {
            await HttpUtils.HandleErrorResponse(response);
            return null!;
        }
    }

    /// <summary>
    /// Queries for work item IDs using WIQL.
    /// </summary>
    /// <param name="filter">Optional filter (not implemented yet).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Array of work item IDs.</returns>
    public async Task<int[]> QueryWorkItemIdsAsync(string? filter = null, CancellationToken ct = default)
    {
        string url = $"{this.azureDevOpsOptions.BaseUrl}/{this.azureDevOpsOptions.Organization}/{this.azureDevOpsOptions.Project}/_apis/wit/wiql?api-version=7.1";

        var query = new
        {
            query = $"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @project ORDER BY [System.ChangedDate] DESC"
        };

        using var content = new StringContent(JsonSerializer.Serialize(query, jsonOptions), Encoding.UTF8, "application/json");
        using HttpResponseMessage response = await this.httpClient.PostAsync(url, content, ct);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<WiqlResponse>(jsonOptions, ct);
            if (result?.WorkItems != null)
            {
                return result.WorkItems.Select(w => w.Id).ToArray();
            }
            return Array.Empty<int>();
        }
        else
        {
            await HttpUtils.HandleErrorResponse(response);
            return Array.Empty<int>();
        }
    }

    /// <summary>
    /// Deletes a work item.
    /// </summary>
    /// <param name="id">The ID of the work item.</param>
    /// <returns>True if the work item was deleted, false otherwise.</returns>
    public async Task<bool> DeleteWorkItemAsync(int id)
    {

        string url = $"{this.azureDevOpsOptions.BaseUrl}/{this.azureDevOpsOptions.Organization}/{this.azureDevOpsOptions.Project}/_apis/wit/workitems/{id}?api-version=7.1";

        using HttpResponseMessage response = await this.httpClient.DeleteAsync(url);

         if (response.IsSuccessStatusCode)
        {
            return true;
        }
        else 
        {
            await HttpUtils.HandleErrorResponse(response);
            return false;
        }
    }

    /// <summary>
    /// Updates a work item.
    /// </summary>
    /// <param name="id">The ID of the work item.</param>
    /// <param name="request">The work item request.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The ID of the updated work item.</returns>
    public async Task<string> UpdateWorkItemAsync(int id, WorkItemRequest request, CancellationToken ct = default)
    {
        string url = $"{this.azureDevOpsOptions.BaseUrl}/{this.azureDevOpsOptions.Organization}/{this.azureDevOpsOptions.Project}/_apis/wit/workitems/{id}?api-version=7.1";

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
            await HttpUtils.HandleErrorResponse(response);
            return null!;
        }
    }
}