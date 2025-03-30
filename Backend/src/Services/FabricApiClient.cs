using System;
using System.Net.Http;
using System.Threading.Tasks;
using Fabric.Rti.workload.Backend.Constants;
using Fabric.Rti.workload.Backend.Contracts.FabricAPIPreview;
using Microsoft.Fabric.Api;
using Microsoft.Fabric.Api.Eventhouse.Models;
using Microsoft.Fabric.Api.Eventstream.Models;
using Microsoft.Fabric.Api.KQLDatabase.Models;

namespace Fabric.Rti.workload.Backend.Services;

public class FabricApiClient : IFabricApiClient
{
    private readonly Uri _fabricBaseUri = new(EnvironmentConstants.FabricApiBaseUrl);
    private readonly IHttpClientService _httpClientService;
    
    public FabricApiClient(IHttpClientService httpClientService)
    {
        _httpClientService = httpClientService;
    }

    public async Task<Eventhouse> CreateEventhouseAsync(Guid workspaceId, string displayName, string token)
    {
        var fabricClient = new FabricClient(token, _fabricBaseUri);
        var createEventhouseRequest = new CreateEventhouseRequest(displayName);

        return await fabricClient.Eventhouse.Items.CreateEventhouseAsync(workspaceId, createEventhouseRequest);
    }

    public async Task<Eventhouse> GetEventhouseAsync(Guid workspaceId, Guid eventhouseId, string token)
    {
        var fabricClient = new FabricClient(token, _fabricBaseUri);

        return await fabricClient.Eventhouse.Items.GetEventhouseAsync(workspaceId, eventhouseId);
    }

    public async Task<KQLDatabase> GetKqlDatabaseAsync(Guid workspaceId, Guid kqlDatabaseId, string token)
    {
        var fabricClient = new FabricClient(token, _fabricBaseUri);

        return await fabricClient.KQLDatabase.Items.GetKQLDatabaseAsync(workspaceId, kqlDatabaseId);
    }

    public async Task<KQLDatabase> UpdateKqlDatabaseAsync(Guid workspaceId, Guid kqlDatabaseId, string newDisplayName, string token)
    {
        var fabricClient = new FabricClient(token, _fabricBaseUri);
        var updateRequest = new UpdateKQLDatabaseRequest
        {
            DisplayName = newDisplayName
        };
        
        return await fabricClient.KQLDatabase.Items.UpdateKQLDatabaseAsync(workspaceId, kqlDatabaseId, updateRequest);
    }

    public async Task<Eventstream> CreateEventstreamAsync(Guid workspaceId, string displayName, string token)
    {
        var fabricClient = new FabricClient(token, _fabricBaseUri);
        var createEventstreamRequest = new CreateEventstreamRequest(displayName);

        return await fabricClient.Eventstream.Items.CreateEventstreamAsync(workspaceId, createEventstreamRequest);
    }
    
    public async Task UpdateEventstreamDefinitionAsync(Guid workspaceId, Guid eventstreamId, EventstreamDefinition eventstreamDefinition, string token)
    {
        var fabricClient = new FabricClient(token, _fabricBaseUri);
        var updateDefinitionRequest = new UpdateEventstreamDefinitionRequest(eventstreamDefinition);

        await fabricClient.Eventstream.Items.UpdateEventstreamDefinitionAsync(workspaceId, eventstreamId, updateDefinitionRequest);
    }

    // TODO - temp till available in Microsoft.Fabric.Api
    public async Task<EventstreamTopologyResponse> GetEventstreamTopologyAsync(Guid workspaceId, Guid eventstreamId, string token)
    {
        var eventstreamTopologyUrl = $"{_fabricBaseUri}/v1/workspaces/{workspaceId}/eventstreams/{eventstreamId}/topology";

        var response = await _httpClientService.GetAsync(eventstreamTopologyUrl, token);
        var topologyResponse = await response.Content.ReadAsAsync<EventstreamTopologyResponse>();
        
        return topologyResponse;
    }
    
    // TODO - temp till available in Microsoft.Fabric.Api
    public async Task<SourceConnectionResponse> GetEventstreamSourceConnectionAsync(Guid workspaceId, Guid eventstreamId, Guid sourceId, string token)
    {
        var eventstreamSourceConnectionUrl = $"{_fabricBaseUri}/v1/workspaces/{workspaceId}/eventstreams/{eventstreamId}/sources/{sourceId}/connection";

        var response = await _httpClientService.GetAsync(eventstreamSourceConnectionUrl, token);
        var sourceConnectionResponse = await response.Content.ReadAsAsync<SourceConnectionResponse>();
        
        return sourceConnectionResponse;
    }
}