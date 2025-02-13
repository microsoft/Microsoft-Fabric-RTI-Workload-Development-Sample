// <copyright company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fabric.Rti.workload.Backend.Constants;
using Fabric.Rti.workload.Backend.Contracts;
using Fabric.Rti.workload.Backend.Exceptions;
using Fabric.Rti.workload.Backend.Services;
using Fabric.Rti.workload.Backend.Utils;
using Microsoft.Extensions.Logging;
using CreateItemPayload = Fabric.Rti.workload.Backend.Contracts.FabricAPI.Workload.CreateItemPayload;
using ItemPayload = Fabric.Rti.workload.Backend.Contracts.FabricAPI.Workload.ItemPayload;
using UpdateItemPayload = Fabric.Rti.workload.Backend.Contracts.FabricAPI.Workload.UpdateItemPayload;

namespace Fabric.Rti.workload.Backend.Items
{
    public class Item1 : ItemBase<Item1, Item1Metadata, Item1ClientMetadata>
    {
        private static readonly IList<string> FabricScopes = new[] { $"{EnvironmentConstants.FabricBackendResourceId}/Lakehouse.Read.All" };
        
        private readonly IAuthenticationService _authenticationService;

        private readonly IFabricApiClient _fabricApiClient;

        private Item1Metadata _metadata;

        public Item1(
            ILogger<Item1> logger,
            IItemMetadataStore itemMetadataStore,
            IAuthenticationService authenticationService,
            IFabricApiClient fabricApiClient,
            AuthorizationContext authorizationContext)
            : base(logger, itemMetadataStore, authorizationContext)
        {
            _authenticationService = authenticationService;
            _fabricApiClient = fabricApiClient;
        }

        public override string ItemType => WorkloadConstants.ItemTypes.Item1;
        
        public override Task<ItemPayload> GetItemPayload()
        {
            var typeSpecificMetadata = GetTypeSpecificMetadata();
            
            return Task.FromResult(new ItemPayload
            {
                Item1Metadata = typeSpecificMetadata.ToClientMetadata()
            });
        }
        
        private Item1Metadata Metadata => Ensure.NotNull(_metadata, "The item object must be initialized before use");
        
        protected override void SetDefinition(CreateItemPayload payload)
        {
            if (payload == null)
            {
                Logger.LogInformation("No payload is provided for {0}, objectId={1}", ItemType, ItemObjectId);
                _metadata = Item1Metadata.Default.Clone();
                return;
            }

            if (payload.Item1Metadata == null)
            {
                throw new InvalidItemPayloadException(ItemType, ItemObjectId);
            }
            
            _metadata = payload.Item1Metadata.Clone();
        }

        protected override void UpdateDefinition(UpdateItemPayload payload)
        {
            if (payload == null)
            {
                Logger.LogInformation("No payload is provided for {0}, objectId={1}", ItemType, ItemObjectId);
                return;
            }

            if (payload.Item1Metadata == null)
            {
                throw new InvalidItemPayloadException(ItemType, ItemObjectId);
            }
            
            SetTypeSpecificMetadata(payload.Item1Metadata);
        }

        protected override void SetTypeSpecificMetadata(Item1Metadata itemMetadata)
        {
            _metadata = itemMetadata.Clone();
        }

        protected override Item1Metadata GetTypeSpecificMetadata()
        {
            return Metadata.Clone();
        }

        protected override async Task CreateAdditionalResourcesAsync()
        {
            var metadata = Metadata.Clone();
            var fabricToken = await GetFabricTokenAsync();
 
            try
            {
                var eventhouseDisplayName = $"{DisplayName}_Eventhouse";
                var kqlDatabaseDisplayName = $"{DisplayName}_KQLDatabase";
                var eventhouseItem = await _fabricApiClient.CreateEventhouse(WorkspaceObjectId, eventhouseDisplayName, fabricToken);
                eventhouseItem = await _fabricApiClient.GetEventhouse(WorkspaceObjectId, eventhouseItem.Id.Value, fabricToken);

                var defaultKqlDatabaseId = eventhouseItem.Properties.DatabasesItemIds.FirstOrDefault();
                await _fabricApiClient.UpdateKqlDatabase(WorkspaceObjectId, defaultKqlDatabaseId, kqlDatabaseDisplayName, fabricToken);
                var kqlDatabaseItem = await _fabricApiClient.GetKqlDatabase(WorkspaceObjectId, defaultKqlDatabaseId, fabricToken);

                metadata.EventhouseItemId = eventhouseItem.Id;
                metadata.EventhouseDisplayName = eventhouseItem.DisplayName;
                metadata.KqlDatabaseItemId = kqlDatabaseItem.Id;
                metadata.KqlDatabaseDisplayName = kqlDatabaseItem.DisplayName;
                metadata.KqlDatabaseQueryUrl = kqlDatabaseItem.Properties.QueryServiceUri;

                _metadata = metadata;
            
                // TODO PrepareKqlDatabaseData
                // fire and forget, prepare initial data on kusto side
                //_ = PrepareKqlDatabaseData(kqlDatabaseItem);
                Logger.LogInformation($"CreateAdditionalResources: successfully create Eventhouse {eventhouseItem.Id} with default KQL database {defaultKqlDatabaseId}");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to create additional resources for item: {DisplayName} in workspace: {WorkspaceObjectId}. Error: {ex.Message}");
                throw;
            }
        }
        
        private async Task<string> GetFabricTokenAsync()
        {
            try
            {
                return await _authenticationService.GetAccessTokenOnBehalfOf(AuthorizationContext, FabricScopes);
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to acquire token for Fabric API. Error: {e.Message}");
                throw;
            }
        }
    }
}
