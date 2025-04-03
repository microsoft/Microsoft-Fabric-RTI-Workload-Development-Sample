using System;
using System.Collections.Generic;
using Fabric.Rti.workload.Backend.Contracts.RtiContracts;
using Microsoft.Fabric.Api.Core.Models;
using Microsoft.Fabric.Api.Eventstream.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using InputSchema = Fabric.Rti.workload.Backend.Contracts.RtiContracts.InputSchema;
using JsonSerializationProperties = Fabric.Rti.workload.Backend.Contracts.RtiContracts.JsonSerializationProperties;
using Schema = Fabric.Rti.workload.Backend.Contracts.RtiContracts.Schema;

namespace Fabric.Rti.workload.Backend.Utils;

public static class EventstreamUtils
{
    private static readonly JsonSerializerSettings s_jsonSerializerSettings = new JsonSerializerSettings
    {
        Formatting = Formatting.None,
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        TypeNameHandling = TypeNameHandling.None,
        TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
        NullValueHandling = NullValueHandling.Include,
        DefaultValueHandling = DefaultValueHandling.Include
    };
    
    public static EventstreamDefinition CreateEventstreamDefinitionWithEventhouseDataConnection(
        string eventstreamName,
        Guid kqlDatabaseWorkspaceId,
        Guid kqlDatabaseItemId,
        string kqlDatabaseName,
        string kqlTableName)
    {
        var payload = CreateEventstreamConfigWithEventhouseDataConnection(
            eventstreamName,
            kqlDatabaseWorkspaceId,
            kqlDatabaseItemId,
            kqlDatabaseName,
            kqlTableName);

        var parts = new List<EventstreamDefinitionPart>
        {
            new()
            {
                Path = "eventstream.json",
                Payload =  Base64Encode(JsonConvert.SerializeObject(payload, s_jsonSerializerSettings)),
                PayloadType = PayloadType.InlineBase64
            }
        };

        return new EventstreamDefinition(parts);
    }

    private static EventstreamConfig CreateEventstreamConfigWithEventhouseDataConnection(
        string eventstreamName,
        Guid kqlDatabaseWorkspaceId,
        Guid kqlDatabaseItemId,
        string kqlDatabaseName,
        string kqlTableName)
    {
        return new EventstreamConfig
        {
            Sources = new List<EventstreamSource>
            {
                new EventstreamSource
                {
                    Name = "customEndpoint1",
                    Type = "CustomEndpoint",
                    Properties = new EmptyProperties()
                }
            },
            Destinations = new List<EventstreamDestination>
            {
                new EventstreamDestination
                {
                    Name = "EventhouseDataConnection",
                    Type = "Eventhouse",
                    Properties = new EventhouseDataConnection
                    {
                        DataIngestionMode = "ProcessedIngestion",
                        WorkspaceId = kqlDatabaseWorkspaceId,
                        ItemId = kqlDatabaseItemId,
                        DatabaseName = kqlDatabaseName,
                        TableName = kqlTableName,
                        InputSerialization = new EventhouseInputSerialization
                        {
                            Type = "Json",
                            Properties = new JsonSerializationProperties
                            {
                                Encoding = "UTF8"
                            }
                        }
                    },
                    InputNodes = new List<EventstreamInputNode>
                    {
                        new EventstreamInputNode
                        {
                            Name = "ManageFields"
                        }
                    },
                    InputSchemas = new List<InputSchema>
                    {
                        new InputSchema
                        {
                            Name = "ManageFields",
                            Schema = new Schema
                            {
                                Columns = new List<Column>
                                {
                                    new Column { Name = "timestamp", Type = "DateTime", Fields = null, Items = null },
                                    new Column { Name = "name", Type = "Nvarchar(max)", Fields = null, Items = null },
                                    new Column { Name = "value", Type = "Float", Fields = null, Items = null }
                                }
                            }
                        }
                    }
                }
            },
            Streams = new List<EventstreamStream>
            {
                new EventstreamStream
                {
                    Name = eventstreamName + "-stream",
                    Type = "DefaultStream",
                    Properties = new EmptyProperties(),
                    InputNodes = new List<EventstreamInputNode>
                    {
                        new EventstreamInputNode
                        {
                            Name = "customEndpoint1"
                        }
                    }
                }
            },
            Operators = new List<EventstreamOperator>
            {
                new EventstreamOperator
                {
                    Name = "ManageFields",
                    Type = "ManageFields",
                    InputNodes = new List<EventstreamInputNode>
                    {
                        new EventstreamInputNode
                        {
                            Name = eventstreamName + "-stream"
                        }
                    },
                    Properties = new OperatorProperties
                    {
                        Columns = new List<OperatorColumn>
                        {
                            new OperatorColumn
                            {
                                Type = "Rename",
                                Properties = new ColumnProperties
                                {
                                    Column = new ColumnReference
                                    {
                                        ExpressionType = "ColumnReference",
                                        Node = null,
                                        ColumnName = "timestamp",
                                        ColumnPathSegments = new List<object>()
                                    }
                                },
                                Alias = "timestamp"
                            },
                            new OperatorColumn
                            {
                                Type = "Rename",
                                Properties = new ColumnProperties
                                {
                                    Column = new ColumnReference
                                    {
                                        ExpressionType = "ColumnReference",
                                        Node = null,
                                        ColumnName = "name",
                                        ColumnPathSegments = new List<object>()
                                    }
                                },
                                Alias = "name"
                            },
                            new OperatorColumn
                            {
                                Type = "Rename",
                                Properties = new ColumnProperties
                                {
                                    Column = new ColumnReference
                                    {
                                        ExpressionType = "ColumnReference",
                                        Node = null,
                                        ColumnName = "value",
                                        ColumnPathSegments = new List<object>()
                                    }
                                },
                                Alias = "value"
                            }
                        }
                    },
                    InputSchemas = new List<InputSchema>
                    {
                        new InputSchema
                        {
                            Name = "es_push-stream",
                            Schema = new Schema
                            {
                                Columns = new List<Column>
                                {
                                    new Column { Name = "timestamp", Type = "DateTime", Fields = null, Items = null },
                                    new Column { Name = "name", Type = "Nvarchar(max)", Fields = null, Items = null },
                                    new Column { Name = "value", Type = "Float", Fields = null, Items = null },
                                    new Column { Name = "EventProcessedUtcTime", Type = "DateTime", Fields = null, Items = null },
                                    new Column { Name = "PartitionId", Type = "BigInt", Fields = null, Items = null },
                                    new Column { Name = "EventEnqueuedUtcTime", Type = "DateTime", Fields = null, Items = null }
                                }
                            }
                        }
                    }
                }
            },
            CompatibilityLevel = "1.0"
        };
    }

    private static string Base64Encode(string payload)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(payload);
        return Convert.ToBase64String(plainTextBytes);
    }
}
