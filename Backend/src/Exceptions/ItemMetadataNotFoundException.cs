// <copyright company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;
using Fabric.Rti.workload.Backend.Constants;
using Fabric.Rti.workload.Backend.Contracts.FabricAPI.Workload;
using Microsoft.AspNetCore.Http;

namespace Fabric.Rti.workload.Backend.Exceptions
{
    public class ItemMetadataNotFoundException: WorkloadExceptionBase
    {
        public ItemMetadataNotFoundException(Guid itemObjectId)
            : base(
                httpStatusCode: StatusCodes.Status404NotFound, 
                errorCode: ErrorCodes.Item.ItemMetadataNotFound, 
                messageTemplate: "Item metadata file cannot be found. It is advised to delete this item and create a new item instead (ItemId: {0})", 
                messageParameters: new[] { itemObjectId.ToString() }, 
                ErrorSource.System,
                isPermanent: true)
        {
        }
    }
}
