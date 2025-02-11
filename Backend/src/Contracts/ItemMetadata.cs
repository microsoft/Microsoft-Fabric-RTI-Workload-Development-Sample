// <copyright company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Fabric.Rti.workload.Backend.Contracts
{
    public class ItemMetadata<TItemMetadata>
    {
        public CommonItemMetadata CommonMetadata { get; init; }

        public TItemMetadata TypeSpecificMetadata { get; init; }
    }
}
