// <copyright company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Fabric.Rti.workload.Backend.Exceptions
{
    public class UnexpectedItemTypeException : InternalErrorException
    {
        public UnexpectedItemTypeException(string message)
            : base(message)
        {
        }
    }
}
