using Newtonsoft.Json;

namespace Fabric.Rti.workload.Backend.Contracts.RtiContracts;

public class EventstreamCustomEndpointAccessKeys
{
    /// <summary> The primary key. </summary>
    [JsonProperty("primaryKey")]
    public string PrimaryKey { get; set; }
    
    /// <summary> The secondary key. </summary>
    [JsonProperty("secondaryKey")]
    public string SecondaryKey { get; }
    
    /// <summary> The primary connection string. </summary>
    [JsonProperty("primaryConnectionString")]
    public string PrimaryConnectionString { get; }
    
    /// <summary> The secondary connection string. </summary>
    [JsonProperty("secondaryConnectionString")]
    public string SecondaryConnectionString { get; }
}