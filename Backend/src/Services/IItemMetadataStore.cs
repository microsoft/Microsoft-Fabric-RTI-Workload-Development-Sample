// <copyright company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using Fabric.Rti.workload.Backend.Contracts;

namespace Fabric.Rti.workload.Backend.Services
{
    public interface IItemMetadataStore
    {
        Task Upsert<TItemMetadata>(Guid tenantObjectId, Guid itemObjectId, CommonItemMetadata commonMetadata, TItemMetadata typeSpecificMetadata);

        Task UpsertJobCancel(Guid tenantObjectId, Guid itemObjectId, string jobType, Guid jobInstanceId, ItemJobMetadata itemJobMetadata);
        Task<ItemMetadata<TItemMetadata>> Load<TItemMetadata>(Guid tenantObjectId, Guid itemObjectId);

        Task Delete(Guid tenantObjectId, Guid itemObjectId);
        bool Exists(Guid tenantObjectId, Guid itemObjectId);
        bool JobCancelRequestExists(Guid tenantObjectId, Guid itemObjectId, Guid jobInstanceId);
    }
}
