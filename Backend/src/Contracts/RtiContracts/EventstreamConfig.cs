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
    public List<EventstreamOperator> Operators { get; set; }

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
    public EventhouseDataConnection Properties { get; set; }

    [JsonPropertyName("inputNodes")]
    public List<EventstreamInputNode> InputNodes { get; set; }

    [JsonPropertyName("inputSchemas")]
    public List<InputSchema> InputSchemas { get; set; }
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

public class EventstreamOperator
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("inputNodes")]
    public List<EventstreamInputNode> InputNodes { get; set; }

    [JsonPropertyName("properties")]
    public OperatorProperties Properties { get; set; }

    [JsonPropertyName("inputSchemas")]
    public List<InputSchema> InputSchemas { get; set; }
}

public class OperatorProperties
{
    [JsonPropertyName("columns")]
    public List<OperatorColumn> Columns { get; set; }
}

public class OperatorColumn
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("properties")]
    public ColumnProperties Properties { get; set; }

    [JsonPropertyName("alias")]
    public string Alias { get; set; }
}

public class ColumnProperties
{
    [JsonPropertyName("column")]
    public ColumnReference Column { get; set; }
}

public class ColumnReference
{
    [JsonPropertyName("expressionType")]
    public string ExpressionType { get; set; }

    [JsonPropertyName("node")]
    public object Node { get; set; }

    [JsonPropertyName("columnName")]
    public string ColumnName { get; set; }

    [JsonPropertyName("columnPathSegments")]
    public List<object> ColumnPathSegments { get; set; }
}

public class EventhouseDataConnection
{
    [JsonPropertyName("dataIngestionMode")]
    public string DataIngestionMode { get; set; }

    [JsonPropertyName("workspaceId")]
    public Guid WorkspaceId { get; set; }

    [JsonPropertyName("itemId")]
    public Guid ItemId { get; set; }

    [JsonPropertyName("databaseName")]
    public string DatabaseName { get; set; }

    [JsonPropertyName("tableName")]
    public string TableName { get; set; }

    [JsonPropertyName("inputSerialization")]
    public EventhouseInputSerialization InputSerialization { get; set; }
}

public class EventhouseInputSerialization
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("properties")]
    public JsonSerializationProperties Properties { get; set; }
}

public class JsonSerializationProperties
{
    [JsonPropertyName("encoding")]
    public string Encoding { get; set; }
}

public class InputSchema
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("schema")]
    public Schema Schema { get; set; }
}

public class Schema
{
    [JsonPropertyName("columns")]
    public List<Column> Columns { get; set; }
}

public class Column
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("fields")]
    public object Fields { get; set; }

    [JsonPropertyName("items")]
    public object Items { get; set; }
}

public class EventstreamInputNode
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class EmptyProperties
{
}