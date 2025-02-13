using System;
using System.Threading.Tasks;
using Fabric.Rti.workload.Backend.Constants;
using Microsoft.Fabric.Api;
using Microsoft.Fabric.Api.Eventhouse.Models;
using Microsoft.Fabric.Api.KQLDatabase.Models;

namespace Fabric.Rti.workload.Backend.Services;

public class FabricApiClient : IFabricApiClient
{
    private readonly Uri _fabricBaseUri = new(EnvironmentConstants.FabricApiBaseUrl);

    public async Task<Eventhouse> CreateEventhouse(Guid workspaceId, string displayName, string token)
    {
        var fabricClient = new FabricClient(token, _fabricBaseUri);
        var createEventhouseRequest = new CreateEventhouseRequest(displayName);

        return await fabricClient.Eventhouse.Items.CreateEventhouseAsync(workspaceId, createEventhouseRequest);
    }

    public async Task<Eventhouse> GetEventhouse(Guid workspaceId, Guid eventhouseId, string token)
    {
        var fabricClient = new FabricClient(token, _fabricBaseUri);

        return await fabricClient.Eventhouse.Items.GetEventhouseAsync(workspaceId, eventhouseId);
    }

    public async Task<KQLDatabase> GetKqlDatabase(Guid workspaceId, Guid kqlDatabaseId, string token)
    {
        var fabricClient = new FabricClient(token, _fabricBaseUri);

        return await fabricClient.KQLDatabase.Items.GetKQLDatabaseAsync(workspaceId, kqlDatabaseId);
    }

    public async Task<KQLDatabase> UpdateKqlDatabase(Guid workspaceId, Guid kqlDatabaseId, string newDisplayName, string token)
    {
        var fabricClient = new FabricClient(token, _fabricBaseUri);
        var updateRequest = new UpdateKQLDatabaseRequest
        {
            DisplayName = newDisplayName
        };
        
        return await fabricClient.KQLDatabase.Items.UpdateKQLDatabaseAsync(workspaceId, kqlDatabaseId, updateRequest);
    }
}