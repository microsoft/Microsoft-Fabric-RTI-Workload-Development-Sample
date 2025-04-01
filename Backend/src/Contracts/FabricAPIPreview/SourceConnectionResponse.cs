using System.Text.Json.Serialization;

namespace Fabric.Rti.workload.Backend.Contracts.FabricAPIPreview;

// TODO This is a subclass of SourceConnectionResponse that is expected to be released in a future package of Microsoft.Fabric.Api

public class SourceConnectionResponse
{
    [JsonPropertyName("fullyQualifiedNamespace")]
    public string FullyQualifiedNamespace { get; set; }

    [JsonPropertyName("eventHubName")] 
    public string EventHubName { get; set; }

    [JsonPropertyName("accessKeys")] 
    public AccessKeys AccessKeys { get; set; }
}

public class AccessKeys
{
    [JsonPropertyName("primaryKey")] 
    public string PrimaryKey { get; set; }

    [JsonPropertyName("secondaryKey")] 
    public string SecondaryKey { get; set; }

    [JsonPropertyName("primaryConnectionString")]
    public string PrimaryConnectionString { get; set; }

    [JsonPropertyName("secondaryConnectionString")]
    public string SecondaryConnectionString { get; set; }
}