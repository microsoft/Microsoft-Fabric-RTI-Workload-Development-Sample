// <copyright company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Fabric.Rti.workload.Backend.Exceptions
{
    public class InvalidRelativePathException : InternalErrorException
    {
        public InvalidRelativePathException(string relativePath)
            : base($"The relative path is invalid: {relativePath}")
        {
        }
    }
}
