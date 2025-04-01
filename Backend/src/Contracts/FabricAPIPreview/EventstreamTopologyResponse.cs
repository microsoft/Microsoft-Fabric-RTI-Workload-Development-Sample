using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Fabric.Rti.workload.Backend.Contracts.FabricAPIPreview;

// TODO This is a subclass of EventstreamTopologyResponse that is expected to be released in a future package of Microsoft.Fabric.Api
public class EventstreamTopologyResponse
{
    [JsonPropertyName("sources")] 
    public IReadOnlyList<SourceResponse> Sources { get; set; }

    [JsonPropertyName("destinations")] 
    public IReadOnlyList<DestinationResponse> Destinations { get; set; }

    [JsonPropertyName("streams")] 
    public IReadOnlyList<object> Streams { get; set; }

    [JsonPropertyName("operators")] 
    public IReadOnlyList<object> Operators { get; set; }

    [JsonPropertyName("compatibilityLevel")]
    public string CompatibilityLevel { get; set; }
}

public class SourceResponse
{
    [JsonPropertyName("id")] 
    public string Id { get; set; }

    [JsonPropertyName("name")] 
    public string Name { get; set; }

    [JsonPropertyName("type")] 
    public string Type { get; set; }
}

public class DestinationResponse
{
    [JsonPropertyName("id")] 
    public string Id { get; set; }

    [JsonPropertyName("name")] 
    public string Name { get; set; }

    [JsonPropertyName("type")] 
    public string Type { get; set; }
}