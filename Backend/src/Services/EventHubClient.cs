using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fabric.Rti.workload.Backend.Services;

public class EventHubClient : IEventHubClient, IAsyncDisposable
{
    private readonly EventHubProducerClient m_eventHubProducerClient;
    
    public EventHubClient(string connectionString)
    {
        m_eventHubProducerClient = new EventHubProducerClient(connectionString);
    }

    public async Task SendAsync(JArray jsonArray, CancellationToken cancellationToken = default)
    {
        using var eventBatch = await m_eventHubProducerClient.CreateBatchAsync(cancellationToken);
        foreach (var jToken in jsonArray)
        {
            var jsonString = JsonConvert.SerializeObject(jToken);
            eventBatch.TryAdd(new EventData(jsonString));
        }

        await m_eventHubProducerClient.SendAsync(eventBatch, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await m_eventHubProducerClient.DisposeAsync();
    }
}