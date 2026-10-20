using System;

namespace Fabric.Rti.workload.Backend.Contracts.RtiContracts;

public class KqlQueryRequest
{
    public Guid WorkspaceId { set; get; }
    public Guid KqlDatabaseItemId { set; get; }
    public string Query { set; get; }
}