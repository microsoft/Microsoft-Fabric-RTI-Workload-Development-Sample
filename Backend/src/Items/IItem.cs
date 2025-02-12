// <copyright company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using Fabric.Rti.workload.Backend.Contracts.FabricAPI.Workload;
using ItemPayload = Fabric.Rti.workload.Backend.Contracts.FabricAPI.Workload.ItemPayload;

namespace Fabric.Rti.workload.Backend.Items
{
    public interface IItem
    {
        public string ItemType { get; }

        public Guid TenantObjectId { get; }

        public Guid WorkspaceObjectId { get; }

        public Guid ItemObjectId { get; }

        public string DisplayName { get; }

        public string Description { get; }

        Task Load(Guid itemId);

        Task<ItemPayload> GetItemPayload();

        Task Create(Guid workspaceId, Guid itemId, CreateItemRequest createItemRequest);

        Task Update(UpdateItemRequest updateItemRequest);

        Task Delete();
    }
}
