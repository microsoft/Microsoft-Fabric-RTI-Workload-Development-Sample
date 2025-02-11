// <copyright company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using Fabric.Rti.workload.Backend.Constants;
using Fabric.Rti.workload.Backend.Contracts.FabricAPI.Workload;
using Microsoft.AspNetCore.Http;

namespace Fabric.Rti.workload.Backend.Exceptions
{
    public class AuthenticationException : WorkloadExceptionBase
    {
        public AuthenticationException(string msalOriginalErrorMessage)
            : base(
                  httpStatusCode: StatusCodes.Status401Unauthorized,
                  errorCode: ErrorCodes.Authentication.AuthError,
                  messageTemplate: msalOriginalErrorMessage,
                  messageParameters: null,
                  errorSource: ErrorSource.External,
                  isPermanent: false)
        {
        }
    }
}
