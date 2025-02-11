// <copyright company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Fabric.Rti.workload.Backend.Contracts
{
    public class FabricItem: ItemReference
    {
        public string Type { get; init; }

        public string DisplayName { get; init; }

        public string Description { get; init; }
    }
}
