using System.Collections.Generic;

namespace Fabric.Rti.workload.Backend.Contracts.FabricAPIPreview;

// TODO This is a subclass of EventstreamTopologyResponse that is expected to be released in a future package of Microsoft.Fabric.Api
public class EventstreamTopologyResponse
{
       public IReadOnlyList<SourceResponse> Sources { get; }
        /// <summary>
        /// A list of Eventstream destinations.
        /// Please note <see cref="DestinationResponse"/> is the base class. According to the scenario, a derived class of the base class might need to be assigned here, or this property needs to be casted to one of the possible derived classes.
        /// The available derived classes include <see cref="ActivatorDestinationResponse"/>, <see cref="CustomEndpointDestinationResponse"/>, <see cref="EventhouseDestinationResponse"/> and <see cref="LakehouseDestinationResponse"/>.
        /// </summary>
        public IReadOnlyList<DestinationResponse> Destinations { get; }
        /// <summary>
        /// A list of Eventstream default and derived streams.
        /// Please note <see cref="StreamResponse"/> is the base class. According to the scenario, a derived class of the base class might need to be assigned here, or this property needs to be casted to one of the possible derived classes.
        /// The available derived classes include <see cref="DefaultStreamResponse"/> and <see cref="DerivedStreamResponse"/>.
        /// </summary>
        public IReadOnlyList<object> Streams { get; }
        /// <summary>
        /// A list of Eventstream operators.
        /// Please note <see cref="Operator"/> is the base class. According to the scenario, a derived class of the base class might need to be assigned here, or this property needs to be casted to one of the possible derived classes.
        /// The available derived classes include <see cref="AggregateOperator"/>, <see cref="ExpandOperator"/>, <see cref="FilterOperator"/>, <see cref="GroupByOperator"/>, <see cref="JoinOperator"/>, <see cref="ManageFieldsOperator"/> and <see cref="UnionOperator"/>.
        /// </summary>
        public IReadOnlyList<object> Operators { get; }
        /// <summary> Represents the compatibility level of the Eventstream topology. Additional compatibility levels may be added over time. </summary>
        public string CompatibilityLevel { get; }
}

public class SourceResponse
{
    public string Id { get; }
    public string Name { get; }
    public string Type { get; }
}

public class DestinationResponse
{
    public string Id { get; }
    public string Name { get; }
    public string Type { get; }
}