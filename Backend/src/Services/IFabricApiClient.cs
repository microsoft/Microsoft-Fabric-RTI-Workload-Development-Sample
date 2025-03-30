using System;
using System.Threading.Tasks;
using Fabric.Rti.workload.Backend.Contracts.FabricAPIPreview;
using Microsoft.Fabric.Api.Eventhouse.Models;
using Microsoft.Fabric.Api.Eventstream.Models;
using Microsoft.Fabric.Api.KQLDatabase.Models;

namespace Fabric.Rti.workload.Backend.Services;

public interface IFabricApiClient
{
    public Task<Eventhouse> CreateEventhouseAsync(Guid workspaceId, string displayName, string token);

    public Task<Eventhouse> GetEventhouseAsync(Guid workspaceId, Guid eventhouseId, string token);

    public Task<KQLDatabase> GetKqlDatabaseAsync(Guid workspaceId, Guid kqlDatabaseId, string token);

    public Task<KQLDatabase> UpdateKqlDatabaseAsync(Guid workspaceId, Guid kqlDatabaseId, string newDisplayName, string token);

    public Task<Eventstream> CreateEventstreamAsync(Guid workspaceId, string displayName, string token);
    
    public Task UpdateEventstreamDefinitionAsync(Guid workspaceId, Guid eventstreamId, EventstreamDefinition eventstreamDefinition, string token);
    
    // This is a preview API that is expected to be released in a future package of Microsoft.Fabric.Api
    public Task<EventstreamTopologyResponse> GetEventstreamTopologyAsync(Guid workspaceId, Guid eventstreamId, string token);

    // This is a preview API that is expected to be released in a future package of Microsoft.Fabric.Api
    public Task<SourceConnectionResponse> GetEventstreamSourceConnectionAsync(Guid workspaceId, Guid eventstreamId, Guid sourceId, string token);
}