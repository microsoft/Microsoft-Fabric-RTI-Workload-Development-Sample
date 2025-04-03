using Newtonsoft.Json.Linq;

namespace Fabric.Rti.workload.Backend.Contracts.RtiContracts;

public class EventstreamSendEventsRequest
{
    public JArray Events { get; set; }
}