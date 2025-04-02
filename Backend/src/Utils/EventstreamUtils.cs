using System;
using System.Collections.Generic;
using Fabric.Rti.workload.Backend.Constants;
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
        string eventstreamName,
        Guid kqlDatabaseWorkspaceId,
        Guid kqlDatabaseItemId,
        string kqlTableName)
    {
        var payload = CreateEventstreamConfigWithEventhouseDataConnection(
            eventstreamName,
            kqlDatabaseWorkspaceId,
            kqlDatabaseItemId,
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
        string kqlTableName)
    {
        return new EventstreamConfig
        {
            Sources =
            [
                new EventstreamSource
                {
                    Name = RtiConstants.EventStreamCustomEndpointSourceName,
                    Type = "CustomEndpoint",
                    Properties = new EmptyProperties()
                }
            ],
            Streams =
            [
                new EventstreamStream
                {
                    Name = eventstreamName + "-stream",
                    Type = "DefaultStream",
                    Properties = new EmptyProperties(),
                    InputNodes =
                    [
                        new EventstreamInputNode
                        {
                            Name = RtiConstants.EventStreamCustomEndpointSourceName
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
                        DataIngestionMode = "DirectIngestion",
                        WorkspaceId = kqlDatabaseWorkspaceId,
                        ItemId = kqlDatabaseItemId,
                        TableName = kqlTableName,
                        ConnectionName = "EventhouseDataConnection",
                        MappingRuleName = RtiConstants.KustoIotDataTableIngestionMappingName
                    },
                    InputNodes =
                    [
                        new EventstreamInputNode
                        {
                            Name = eventstreamName + "-stream"
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
