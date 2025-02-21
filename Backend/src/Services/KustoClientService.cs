using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Kusto.Data;
using Kusto.Data.Common;
using Kusto.Ingest;

namespace Fabric.Rti.workload.Backend.Services;

public class KustoClientService : IKustoClientService
{
    private readonly IKustoStatelessClient _kustoStatelessClient;

    public KustoClientService(IKustoStatelessClient kustoStatelessClient)
    {
        _kustoStatelessClient = kustoStatelessClient;
    }

    public async Task<IDataReader> ExecuteQueryAsync(
        string queryUrl,
        string databaseName,
        string query,
        ClientRequestProperties clientRequestProperties = null,
        CancellationToken cancellationToken = default)
    {
        return await _kustoStatelessClient.ExecuteQueryAsync(
            queryUrl,
            databaseName,
            query,
            clientRequestProperties,
            cancellationToken);
    }

    public async Task<IDataReader> ExecuteControlCommandAsync(
        string queryUrl,
        string databaseName,
        string command,
        ClientRequestProperties clientRequestProperties = null,
        CancellationToken cancellationToken = default)
    {
        return await _kustoStatelessClient.ExecuteControlCommandAsync(
            queryUrl,
            databaseName,
            command,
            clientRequestProperties,
            cancellationToken);
    }

    public async Task<IKustoIngestionResult> IngestFromStreamAsync(string ingestionUrl, Stream stream, KustoIngestionProperties ingestionProperties, string token, StreamSourceOptions sourceOptions = null)
    {
        var connectionStringBuilder = new KustoConnectionStringBuilder(ingestionUrl).WithAadUserTokenAuthentication(token);
        using var ingestClient = KustoIngestFactory.CreateQueuedIngestClient(connectionStringBuilder);

        return await ingestClient.IngestFromStreamAsync(stream, ingestionProperties);
    }
}