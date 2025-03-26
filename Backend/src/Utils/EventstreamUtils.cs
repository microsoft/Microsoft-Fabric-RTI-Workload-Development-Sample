using System;
using System.Collections.Generic;
using Fabric.Rti.workload.Backend.Contracts.RtiContracts;
using Microsoft.Fabric.Api.Core.Models;
using Microsoft.Fabric.Api.Eventstream.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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
        Guid kqlDatabaseWorkspaceId,
        Guid kqlDatabaseItemId,
        string kqlDatabaseDisplayName,
        string kqlTableName)
    {
        var payload = CreateEventstreamConfigWithEventhouseDataConnection(
            kqlDatabaseWorkspaceId,
            kqlDatabaseItemId,
            kqlDatabaseDisplayName,
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
        Guid kqlDatabaseWorkspaceId,
        Guid kqlDatabaseItemId,
        string kqlDatabaseDisplayName,
        string kqlTableName)
    {
        return new EventstreamConfig
        {
            Sources =
            [
                new EventstreamSource
                {
                    Name = "customEndpoint1",
                    Type = "CustomEndpoint",
                    Properties = new EmptyProperties()
                }
            ],
            Streams =
            [
                new EventstreamStream
                {
                    Name = "stream1",
                    Type = "DefaultStream",
                    Properties = new EmptyProperties(),
                    InputNodes =
                    [
                        new EventstreamInputNode
                        {
                            Name = "customEndpoint1"
                        }
                    ]
                }
            ],
            Destinations =
            [
                new EventstreamDestination
                {
                    Name = "EventhouseDataConnection",
                    Type = "Eventhouse",
                    Properties = new EventhouseDataConnection
                    {
                        DataIngestionMode = "ProcessedIngestion",
                        WorkspaceId = kqlDatabaseWorkspaceId,
                        ItemId = kqlDatabaseItemId,
                        DatabaseName = kqlDatabaseDisplayName,
                        TableName = kqlTableName,
                        InputSerialization = new EventhouseInputSerialization
                        {
                            Type = "Json",
                            Properties = new Dictionary<string, string> {
                                { "encoding", "UTF8"}
                            }
                        }
                    },
                    InputNodes =
                    [
                        new EventstreamInputNode
                        {
                            Name = "stream1"
                        }
                    ],
                    InputSchemas = []
                }
            ],
            Operators = [],
            CompatibilityLevel = "1.0"
        };
    }
    
    private static string Base64Encode(string payload)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(payload);
        return Convert.ToBase64String(plainTextBytes);
    }
}
