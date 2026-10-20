using System;

namespace Fabric.Rti.workload.Backend.Contracts.RtiContracts;

public class KqlIngestRequest
{
    public Guid WorkspaceId { set; get; }
    public Guid KqlDatabaseItemId { set; get; }
    public string TableName { get; set; }
    public string IngestionMappingName { get; set; } 
    public string Content { set; get; }
}