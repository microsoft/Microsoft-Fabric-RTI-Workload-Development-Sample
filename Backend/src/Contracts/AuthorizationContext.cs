// <copyright company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Security.Claims;
using Fabric.Rti.workload.Backend.Utils;

namespace Fabric.Rti.workload.Backend.Contracts
{
    public class AuthorizationContext
    {
        public string OriginalSubjectToken { get; init; }

        public IList<Claim> Claims { get; init; }

        public Guid TenantObjectId { get; init; }

        public Guid ObjectId => new Guid(Claims.GetClaimValue("oid"));

        public bool HasSubjectContext => !string.IsNullOrEmpty(OriginalSubjectToken);
    }
}
