using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Fabric.Rti.workload.Backend.Contracts.RtiContracts;

public class EventstreamConfig
{
    [JsonPropertyName("sources")]
    public List<EventstreamSource> Sources { get; set; }

    [JsonPropertyName("destinations")]
    public List<EventstreamDestination> Destinations { get; set; }

    [JsonPropertyName("streams")]
    public List<EventstreamStream> Streams { get; set; }

    [JsonPropertyName("operators")]
    public List<object> Operators { get; set; }

    [JsonPropertyName("compatibilityLevel")]
    public string CompatibilityLevel { get; set; }
}

public class EventstreamSource
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("properties")]
    public EmptyProperties Properties { get; set; }
}

public class EventstreamDestination
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("properties")]
    public object Properties { get; set; }
    
    [JsonPropertyName("inputNodes")]
    public List<EventstreamInputNode> InputNodes { get; set; }
    
    [JsonPropertyName("inputSchemas")]
    public List<object> InputSchemas { get; set; }
}

public class EventstreamStream
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("properties")]
    public EmptyProperties Properties { get; set; }
    
    [JsonPropertyName("inputNodes")]
    public List<EventstreamInputNode> InputNodes { get; set; }
}

public class EventstreamInputNode
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class EventhouseDataConnection
{
    [JsonPropertyName("dataIngestionMode")]
    public string DataIngestionMode { get;  set; }
    
    [JsonPropertyName("workspaceId")]
    public Guid WorkspaceId { get; set; }
    
    [JsonPropertyName("itemId")]
    public Guid ItemId { get; set; }
    
    [JsonPropertyName("tableName")]
    public string TableName { get; set; }
    
    [JsonPropertyName("connectionName")]
    public string ConnectionName { get; set; }
}

public class EventhouseInputSerialization
{
    [JsonPropertyName("type")]
    public string Type { get; set; }
    
    [JsonPropertyName("properties")]
    public object Properties { get; set; }
}

public class EmptyProperties
{
}

