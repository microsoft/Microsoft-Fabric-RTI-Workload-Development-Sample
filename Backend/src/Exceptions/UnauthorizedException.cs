// <copyright company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using Fabric.Rti.workload.Backend.Constants;
using Fabric.Rti.workload.Backend.Contracts.FabricAPI.Workload;
using Microsoft.AspNetCore.Http;

namespace Fabric.Rti.workload.Backend.Exceptions
{
    public class UnauthorizedException : WorkloadExceptionBase
    {
        public UnauthorizedException()
            : base(
                  httpStatusCode: StatusCodes.Status403Forbidden,   // Due to security considerations for real-world applications returning '404 Not Found' or '401 Unauthorized' may be more appropriate 
                  errorCode: ErrorCodes.Security.AccessDenied,
                  messageTemplate: "Access denied",
                  messageParameters: null,
                  errorSource: ErrorSource.User,
                  isPermanent: true)
        {
        }
    }
}
