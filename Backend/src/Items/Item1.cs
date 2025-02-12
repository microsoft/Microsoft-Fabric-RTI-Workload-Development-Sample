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
    public class Item1 : ItemBase<Item1, Item1Metadata, Item1ClientMetadata>, IItem1
    {
        public static readonly IList<string> SupportedOperators = Enum.GetNames(typeof(Item1Operator))
            .Where(name => name != nameof(Item1Operator.Undefined)).ToList();

        private static readonly IList<string> OneLakeScopes = new[] { $"{EnvironmentConstants.OneLakeResourceId}/.default" };

        private static readonly IList<string> FabricScopes = new[] { $"{EnvironmentConstants.FabricBackendResourceId}/Lakehouse.Read.All" };
        
        private readonly IAuthenticationService _authenticationService;

        private readonly IItemMetadataStore _itemMetadataStore;

        private Item1Metadata _metadata;

        public Item1(
            ILogger<Item1> logger,
            IItemMetadataStore itemMetadataStore,
            IAuthenticationService authenticationService,
            AuthorizationContext authorizationContext)
            : base(logger, itemMetadataStore, authorizationContext)
        {
            _authenticationService = authenticationService;
            _itemMetadataStore = itemMetadataStore;
        }

        public override string ItemType => WorkloadConstants.ItemTypes.Item1;

        public ItemReference Lakehouse => Metadata.Lakehouse;

        public int Operand1 => Metadata.Operand1;

        public int Operand2 => Metadata.Operand2;

        public override async Task<ItemPayload> GetItemPayload()
        {
            var typeSpecificMetadata = GetTypeSpecificMetadata();

            FabricItem lakehouseItem = null;
            if (typeSpecificMetadata.Lakehouse.Id != Guid.Empty)
            {
                try
                {
                   // var token = await _authenticationService.GetAccessTokenOnBehalfOf(AuthorizationContext, FabricScopes);
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Failed to retrieve FabricLakehouse for lakehouse: {typeSpecificMetadata.Lakehouse.Id} in workspace: {typeSpecificMetadata.Lakehouse.WorkspaceId}. Error: {ex.Message}");
                }
            }

            return new ItemPayload
            {
                Item1Metadata = typeSpecificMetadata.ToClientMetadata(lakehouseItem)
            };
        }
        
        public Item1Operator Operator => Metadata.Operator;

        private Item1Metadata Metadata => Ensure.NotNull(_metadata, "The item object must be initialized before use");

        private void ValidateOperandsBeforeDouble(int operand1, int operand2)
        {
            var invalidOperands = new List<string>();
            if (operand1 > int.MaxValue / 2 || operand1 < int.MinValue / 2)
            {
                invalidOperands.Add("Operand1");
            }
            if (operand2 > int.MaxValue / 2 || operand2 < int.MinValue / 2)
            {
                invalidOperands.Add("Operand2");
            }
            if (!invalidOperands.IsNullOrEmpty())
            {
                string joinedInvalidOperands = string.Join(", ", invalidOperands);
                throw new DoubledOperandsOverflowException(new List<string> { joinedInvalidOperands });
            }
        }

        public async Task<(int Operand1, int Operand2)> Double()
        {
            var metadata = Metadata.Clone();

            ValidateOperandsBeforeDouble(metadata.Operand1, metadata.Operand2);
            metadata.Operand1 *= 2;
            metadata.Operand2 *= 2;

            _metadata = metadata;

            await SaveChanges();

            return (metadata.Operand1, metadata.Operand2);
        }

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

            if (payload.Item1Metadata.Lakehouse == null)
            {
                throw new InvalidItemPayloadException(ItemType, ItemObjectId)
                    .WithDetail(ErrorCodes.ItemPayload.MissingLakehouseReference, "Missing Lakehouse reference");
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

            if (payload.Item1Metadata.Lakehouse == null)
            {
                throw new InvalidItemPayloadException(ItemType, ItemObjectId)
                    .WithDetail(ErrorCodes.ItemPayload.MissingLakehouseReference, "Missing Lakehouse reference");
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
    }
}
