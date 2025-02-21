namespace Fabric.Rti.workload.Backend.Contracts.RtiContracts;

public class KqlManagementCommandRequest
{
    public string KqlDatabaseQueryUrl { set; get; }
    public string KqlDatabaseItemId { set; get; }
    public string Command { set; get; }
}