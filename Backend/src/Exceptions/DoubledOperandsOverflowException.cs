// <copyright company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Fabric.Rti.workload.Backend.Constants;
using Fabric.Rti.workload.Backend.Contracts.FabricAPI.Workload;
using Microsoft.AspNetCore.Http;

namespace Fabric.Rti.workload.Backend.Exceptions
{
    public class DoubledOperandsOverflowException : WorkloadExceptionBase
    {
        public DoubledOperandsOverflowException(IList<string> messageParameters)
            : base(
                  httpStatusCode: StatusCodes.Status400BadRequest,
                  errorCode: ErrorCodes.Item.DoubledOperandsOverflow,
                  messageTemplate: "{0} may lead to overflow",
                  messageParameters: messageParameters,
                  errorSource: ErrorSource.User,
                  isPermanent: false)
        {
        }
    }
}
