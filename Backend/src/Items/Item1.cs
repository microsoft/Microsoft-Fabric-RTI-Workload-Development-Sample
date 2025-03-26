// <copyright company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fabric.Rti.workload.Backend.Constants;
using Fabric.Rti.workload.Backend.Contracts;
using Fabric.Rti.workload.Backend.Contracts.RtiContracts;
using Fabric.Rti.workload.Backend.Exceptions;
using Fabric.Rti.workload.Backend.Services;
using Fabric.Rti.workload.Backend.Utils;
using Kusto.Data.Common;
using Microsoft.Extensions.Logging;
using CreateItemPayload = Fabric.Rti.workload.Backend.Contracts.FabricAPI.Workload.CreateItemPayload;
using ItemPayload = Fabric.Rti.workload.Backend.Contracts.FabricAPI.Workload.ItemPayload;
using UpdateItemPayload = Fabric.Rti.workload.Backend.Contracts.FabricAPI.Workload.UpdateItemPayload;

namespace Fabric.Rti.workload.Backend.Items
{
    public class Item1 : ItemBase<Item1, Item1Metadata, Item1ClientMetadata>
    {
        private static readonly IList<string> FabricScopes = new[] { $"{EnvironmentConstants.FabricBackendResourceId}/{WorkloadScopes.KQLDatabaseReadWriteAll}" };

        private readonly IAuthenticationService _authenticationService;

        private readonly IFabricApiClient _fabricApiClient;
        
        private readonly IKustoClientService _kustoClientService;

        private Item1Metadata _metadata;

        public Item1(
            ILogger<Item1> logger,
            IItemMetadataStore itemMetadataStore,
            IAuthenticationService authenticationService,
            IFabricApiClient fabricApiClient,
            IKustoClientService kustoClientService,
            AuthorizationContext authorizationContext)
            : base(logger, itemMetadataStore, authorizationContext)
        {
            _authenticationService = authenticationService;
            _fabricApiClient = fabricApiClient;
            _kustoClientService = kustoClientService;
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
                var eventstreamDisplayName = $"{DisplayName}_Eventstream";
                
                var eventhouseItemTask = _fabricApiClient.CreateEventhouseAsync(WorkspaceObjectId, eventhouseDisplayName, fabricToken);
                var eventstreamItemTask =  _fabricApiClient.CreateEventstreamAsync(WorkspaceObjectId, eventstreamDisplayName, fabricToken);
                
                var itemsCreationTasks = new List<Task>
                {
                    eventhouseItemTask,
                    eventstreamItemTask
                };

                await Task.WhenAll(itemsCreationTasks);
                
                var eventhouseItem = await _fabricApiClient.GetEventhouseAsync(WorkspaceObjectId, eventhouseItemTask.Result.Id.Value, fabricToken);

                var defaultKqlDatabaseId = eventhouseItem.Properties.DatabasesItemIds.FirstOrDefault();
                await _fabricApiClient.UpdateKqlDatabaseAsync(WorkspaceObjectId, defaultKqlDatabaseId, kqlDatabaseDisplayName, fabricToken);
                var kqlDatabaseItem = await _fabricApiClient.GetKqlDatabaseAsync(WorkspaceObjectId, defaultKqlDatabaseId, fabricToken);
                
                metadata.EventhouseItemId = eventhouseItem.Id;
                metadata.EventhouseDisplayName = eventhouseItem.DisplayName;
                metadata.KqlDatabaseItemId = kqlDatabaseItem.Id;
                metadata.KqlDatabaseDisplayName = kqlDatabaseItem.DisplayName;
                metadata.KqlDatabaseQueryUrl = kqlDatabaseItem.Properties.QueryServiceUri;
                metadata.KqlDatabaseIngestionUrl = kqlDatabaseItem.Properties.IngestionServiceUri;
                metadata.EventstreamItemId = eventstreamItemTask.Result.Id;
                metadata.EventstreamDisplayName = eventstreamItemTask.Result.DisplayName;

                _metadata = metadata;

                // fire and forget, prepare and setup additional resources in the background
                _ = PrepareAdditionalResourcesAsync(metadata, fabricToken);
                Logger.LogInformation($"CreateAdditionalResources: successfully create Eventhouse {eventhouseItem.Id} with default KQL database {defaultKqlDatabaseId}" +
                    $" and Eventstream {eventstreamItemTask.Result.Id} in workspace {WorkspaceObjectId}");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to create additional resources for item: {DisplayName} in workspace: {WorkspaceObjectId}. Error: {ex.Message}");
                throw;
            }
        }

        private async Task PrepareAdditionalResourcesAsync(Item1Metadata metadata, string fabricToken)
        {
            await PrepareKqlDatabaseData(metadata);
            await SetupEventStreamDataConnectionAsync(metadata, fabricToken);
        }

        private async Task PrepareKqlDatabaseData(Item1Metadata metadata)
        {
            var kqlDatabaseQueryUrl = metadata.KqlDatabaseQueryUrl;
            var kqlDatabaseItemId = metadata.KqlDatabaseItemId.ToString();

            try
            {
                Logger.LogInformation($"PrepareKqlDatabaseData: getting kusto data plane token for kql query url: {kqlDatabaseQueryUrl}");
                var kustoDataPlaneToken = await GetKustoDataPlaneTokenAsync(kqlDatabaseQueryUrl);
                var kustoClientRequestProperties = new ClientRequestProperties
                {
                    AuthorizationScheme = "Bearer",
                    SecurityToken = kustoDataPlaneToken
                };

                Logger.LogInformation($"PrepareKqlDatabaseData: creating table {RtiConstants.KustoIotDataTableName} in kql database {kqlDatabaseItemId}");
                var tableCreateCommand = CslCommandGenerator.GenerateTableCreateCommand(RtiConstants.KustoIotDataTableName, typeof(KustoIotDataTableRecord), forceNormalizeColumnName: false);
                await _kustoClientService.ExecuteControlCommandAsync(kqlDatabaseQueryUrl, kqlDatabaseItemId, tableCreateCommand, kustoClientRequestProperties, default);

                Logger.LogInformation($"PrepareKqlDatabaseData: ingesting initial data to table {RtiConstants.KustoIotDataTableName} in kql database {kqlDatabaseItemId}");
                var records = KustoIotDataTableRecordExtensions.GenerateRandomRecords(3);
                var csvData = string.Join(Environment.NewLine, records.Select(r => r.ToCsvFormat()));
                var ingestCommand = CslCommandGenerator.GenerateTableIngestPushCommand(RtiConstants.KustoIotDataTableName, compressed: false, csvData);
                await _kustoClientService.ExecuteControlCommandAsync(kqlDatabaseQueryUrl, kqlDatabaseItemId, ingestCommand, kustoClientRequestProperties, default);
                
                Logger.LogInformation($"PrepareKqlDatabaseData: successfully prepared kql database data for database id {kqlDatabaseItemId}");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed preparing kql database data for database id {kqlDatabaseItemId}. Error: {ex.Message}");
            }
        }

        private async Task SetupEventStreamDataConnectionAsync(Item1Metadata metadata, string fabricToken)
        {
            try
            {
                var eventstreamDefinition = EventstreamUtils.CreateEventstreamDefinitionWithEventhouseDataConnection(
                    WorkspaceObjectId,
                    metadata.KqlDatabaseItemId.Value,
                    metadata.KqlDatabaseDisplayName,
                    RtiConstants.KustoIotDataTableName);

                await _fabricApiClient.UpdateEventstreamDefinitionAsync(
                    WorkspaceObjectId,
                    metadata.EventstreamItemId.Value,
                    eventstreamDefinition,
                    fabricToken);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to set up event stream data connection. Error: {ex.Message}");
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

        private async Task<string> GetKustoDataPlaneTokenAsync(string kqlDatabaseQueryUrl)
        {
            var scopes = new[] { $"{kqlDatabaseQueryUrl}/.default" };
            return await _authenticationService.GetAccessTokenOnBehalfOf(AuthorizationContext, scopes);
        }
    }
}
