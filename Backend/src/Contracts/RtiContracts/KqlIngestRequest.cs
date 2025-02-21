namespace Fabric.Rti.workload.Backend.Contracts.RtiContracts;

public class KqlIngestRequest
{
    public string IngestionServiceUri { set; get; }
    public string KqlDatabaseItemId { set; get; }
    public string TableName { get; set; }
    public string IngestionMappingName { get; set; } 
    public string Content { set; get; }
}