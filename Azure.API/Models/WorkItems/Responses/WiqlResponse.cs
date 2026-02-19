using System.Text.Json.Serialization;

namespace Azure.API.Models.WorkItems.Responses;

public class WiqlResponse
{
    [JsonPropertyName("queryType")]
    public string QueryType { get; set; }

    [JsonPropertyName("asOf")]
    public DateTime AsOf { get; set; }

    [JsonPropertyName("workItems")]
    public List<WorkItemReference> WorkItems { get; set; }
}

public class WorkItemReference
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }
}
