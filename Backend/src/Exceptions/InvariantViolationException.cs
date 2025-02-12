// <copyright company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Fabric.Rti.workload.Backend.Exceptions
{
    public class InvariantViolationException : InternalErrorException
    {
        public InvariantViolationException(string message)
            : base(message)
        {
        }

        public override string ToTelemetryString()
        {
            return $"INVARIANT VIOLATION: {InternalMessage}";
        }
    }
}
