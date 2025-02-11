// <copyright company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using Fabric.Rti.workload.Backend.Constants;
using Fabric.Rti.workload.Backend.Contracts.FabricAPI.Workload;
using Microsoft.AspNetCore.Http;

namespace Fabric.Rti.workload.Backend.Exceptions
{
    public class TooManyRequestsException : WorkloadExceptionBase
    {
        public TooManyRequestsException(string message = "Too many requests")
        : base(
              httpStatusCode: StatusCodes.Status429TooManyRequests,
              errorCode: ErrorCodes.RateLimiting.TooManyRequests,
              messageTemplate: message,
              messageParameters: null,
              errorSource: ErrorSource.User,
              isPermanent: false)
        {
        }
    }
}