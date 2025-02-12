// <copyright company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using Fabric.Rti.workload.Backend.Contracts;
using Fabric.Rti.workload.Backend.Items;

namespace Fabric.Rti.workload.Backend.Services
{
    public interface IItemFactory
    {
        IItem CreateItem(string itemType, AuthorizationContext authorizationContext);
    }
}
