using System;

namespace Fabric.Rti.workload.Backend.Contracts.RtiContracts;

public class KqlManagementCommandRequest
{
    public Guid WorkspaceId { set; get; }
    public Guid KqlDatabaseItemId { set; get; }
    public string Command { set; get; }
}