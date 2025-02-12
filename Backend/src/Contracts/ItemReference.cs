// <copyright company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;

namespace Fabric.Rti.workload.Backend.Contracts
{
    public class ItemReference
    {
        public Guid WorkspaceId { get; init; }

        public Guid Id { get; init; }
    }
}
