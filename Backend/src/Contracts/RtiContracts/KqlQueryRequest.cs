namespace Fabric.Rti.workload.Backend.Contracts.RtiContracts;

public class KqlQueryRequest
{
    public string KqlDatabaseQueryUrl { set; get; }
    public string KqlDatabaseItemId { set; get; }
    public string Query { set; get; }
}