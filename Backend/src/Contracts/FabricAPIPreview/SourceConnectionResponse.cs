namespace Fabric.Rti.workload.Backend.Contracts.FabricAPIPreview;

// TODO This is a subclass of SourceConnectionResponse that is expected to be released in a future package of Microsoft.Fabric.Api

public class SourceConnectionResponse
{
    /// <summary> The fully qualified namespace of the EventHub. </summary>
    public string FullyQualifiedNamespace { get; }
    /// <summary> The name of the EventHub. </summary>
    public string EventHubName { get; }
    /// <summary> The access keys. </summary>
    public AccessKeys AccessKeys { get; }
}

public class AccessKeys
{
    /// <summary> The primary key. </summary>
    public string PrimaryKey { get; }
    /// <summary> The secondary key. </summary>
    public string SecondaryKey { get; }
    /// <summary> The primary connection string. </summary>
    public string PrimaryConnectionString { get; }
    /// <summary> The secondary connection string. </summary>
    public string SecondaryConnectionString { get; }
}