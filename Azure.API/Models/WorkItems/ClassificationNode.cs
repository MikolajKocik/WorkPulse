using System.Text.Json.Serialization;
#pragma warning disable CS8618

namespace Azure.API.Models.WorkItems;

public class ClassificationNode
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("structureType")]
    public string StructureType { get; set; }

    [JsonPropertyName("hasChildren")]
    public bool HasChildren { get; set; }

    [JsonPropertyName("children")]
    public List<ClassificationNode> Children { get; set; }

    [JsonPropertyName("path")]
    public string Path { get; set; }
}

public class ClassificationNodeList
{
    [JsonPropertyName("value")]
    public List<ClassificationNode> Value { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; }
}
#pragma warning restore CS8618
