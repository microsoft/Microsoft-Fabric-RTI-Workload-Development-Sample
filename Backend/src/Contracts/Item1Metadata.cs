// <copyright company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;

namespace Fabric.Rti.workload.Backend.Contracts
{
    public abstract class Item1MetadataBase
    {
        public Guid? EventhouseItemId { get; set; }
    
        public string EventhouseDisplayName { get; set; }
    
        public Guid? KqlDatabaseItemId { get; set; }
    
        public string KqlDatabaseDisplayName { get; set; }
    
        public string KqlDatabaseQueryUrl { get; set; }
    }

    /// <summary>
    /// Represents the core metadata for item1 stored within the system's storage.
    /// </summary>
    public class Item1Metadata: Item1MetadataBase
    {
        public static readonly Item1Metadata Default = new();

        public Item1Metadata Clone()
        {
            return new Item1Metadata
            {
                EventhouseItemId = EventhouseItemId,
                EventhouseDisplayName = EventhouseDisplayName,
                KqlDatabaseItemId = KqlDatabaseItemId,
                KqlDatabaseDisplayName = KqlDatabaseDisplayName,
                KqlDatabaseQueryUrl = KqlDatabaseQueryUrl
            };
        }

        public Item1ClientMetadata ToClientMetadata()
        {
            return new Item1ClientMetadata
            {
                EventhouseItemId = EventhouseItemId,
                EventhouseDisplayName = EventhouseDisplayName,
                KqlDatabaseItemId = KqlDatabaseItemId,
                KqlDatabaseDisplayName = KqlDatabaseDisplayName,
                KqlDatabaseQueryUrl = KqlDatabaseQueryUrl
            };
        }
    }

    /// <summary>
    /// Represents extended metadata for item1, including additional information
    /// about the associated lakehouse, tailored for client-side usage.
    /// </summary>
    public class Item1ClientMetadata : Item1MetadataBase { }
}
