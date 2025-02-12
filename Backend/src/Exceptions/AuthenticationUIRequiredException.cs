// <copyright company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System.Linq;
using Fabric.Rti.workload.Backend.Constants;
using Fabric.Rti.workload.Backend.Contracts.FabricAPI.Workload;
using Microsoft.AspNetCore.Http;

namespace Fabric.Rti.workload.Backend.Exceptions
{
    public class AuthenticationUIRequiredException : WorkloadExceptionBase
    {
        public static string AdditionalScopesToConsentName = "additionalScopesToConsent";
        public static string ClaimsForCondtionalAccessPolicyName = "claimsForCondtionalAccessPolicy";
        public AuthenticationUIRequiredException(string msalOriginalErrorMessage)
            : base(
                  httpStatusCode: StatusCodes.Status401Unauthorized,
                  errorCode: ErrorCodes.Authentication.AuthUIRequired,
                  messageTemplate: msalOriginalErrorMessage,
                  messageParameters: null,
                  errorSource: ErrorSource.System,
                  isPermanent: false)
        {
        }

        public string ClaimsForConditionalAccessPolicy => Details?.FirstOrDefault()?.AdditionalParameters?.Where(ap => ap.Name == ClaimsForCondtionalAccessPolicyName).FirstOrDefault()?.Value;
    }
}
