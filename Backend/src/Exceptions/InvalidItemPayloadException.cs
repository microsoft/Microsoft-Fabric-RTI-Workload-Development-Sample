// <copyright company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;
using Fabric.Rti.workload.Backend.Constants;
using Fabric.Rti.workload.Backend.Contracts.FabricAPI.Workload;
using Microsoft.AspNetCore.Http;

namespace Fabric.Rti.workload.Backend.Exceptions
{
    public class InvalidItemPayloadException : WorkloadExceptionBase
    {
        public InvalidItemPayloadException(string itemType, Guid id)
            : base(
                  httpStatusCode: StatusCodes.Status400BadRequest,
                  errorCode: ErrorCodes.ItemPayload.InvalidItemPayload,
                  messageTemplate: "{0} payload is invalid for id={1}. See MoreDetails for additional information.",
                  messageParameters: new[] { itemType, id.ToString() },
                  errorSource: ErrorSource.User,
                  isPermanent: true)
        {
        }
    }
}
