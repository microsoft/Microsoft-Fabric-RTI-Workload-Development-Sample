using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Kusto.Data.Common;

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
            cancellationToken).ConfigureAwait(false);
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
            cancellationToken).ConfigureAwait(false);
    }
}