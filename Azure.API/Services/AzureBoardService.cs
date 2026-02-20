using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using Azure.API.Config;
using Azure.API.Utils;
using Azure.API.Models.WorkItems;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Azure.API.Services.Interfaces;

namespace Azure.API.Services;

public class AzureBoardService : IAzureBoardService
{
    private readonly AzureDevOpsOptions azureDevOpsOptions;
    private readonly HttpClient httpClient;
    private readonly ILogger<AzureBoardService> logger;

    public AzureBoardService(
        IOptions<AzureDevOpsOptions> azureDevOpsOptions,

        IHttpClientFactory httpClientFactory,
        ILogger<AzureBoardService> logger
        )
    {
        this.azureDevOpsOptions = azureDevOpsOptions.Value;
        this.httpClient = httpClientFactory.CreateClient("AzureDevOps");
        this.logger = logger;
    }

    /// <summary>
    /// Gets the classification nodes.
    /// </summary>
    /// <param name="structureGroup">The structure group.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The classification nodes.</returns>
    public async Task<List<string>> GetClassificationNodesAsync(string structureGroup, CancellationToken ct = default)
    {
        string url = $"{this.azureDevOpsOptions.BaseUrl}/{this.azureDevOpsOptions.Organization}/{this.azureDevOpsOptions.Project}/_apis/wit/classificationnodes/{structureGroup}?api-version=7.1&$depth=5";

        using HttpResponseMessage response = await this.httpClient.GetAsync(url, ct);

        if (response.IsSuccessStatusCode)
        {
            try 
            {
                var result = await response.Content.ReadFromJsonAsync<ClassificationNode>(cancellationToken: ct);
                var paths = new List<string>();
                if (result != null)
                {
                    AzureBoardUtils.FlattenNodePath(result, paths);
                }
                return paths;
            }
            catch (JsonException ex)
            {
                 logger.LogError(ex, "Failed to parse classification nodes.");
                 return new List<string>();
            }
        }
        else
        {
            logger.LogError("Failed to get classification nodes: {StatusCode}", response.StatusCode);
            return new List<string>();
        }
    }
}