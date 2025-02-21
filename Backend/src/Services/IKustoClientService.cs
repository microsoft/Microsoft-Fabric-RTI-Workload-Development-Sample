using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Kusto.Data.Common;
using Kusto.Ingest;

namespace Fabric.Rti.workload.Backend.Services;

/// <summary>
/// Interface for the Kusto Client Service implementing the Kusto REST API.
/// https://learn.microsoft.com/en-us/kusto/api/rest/?view=microsoft-fabric
/// </summary>
public interface IKustoClientService
{
    /// <summary>
    /// Executes a query against the Kusto database.
    /// </summary>
    /// <param name="queryUrl">The query url of the service e.g https://trd-uvbhpjmntkuc1b61qx.z1.kusto.fabric.microsoft.com</param>
    /// <param name="databaseItemId">The database Item id, alternatively can be the database display name for some cases e.g 4e6a777c-706f-4de4-a5c4-5b73c1ab2f5f </param>
    /// <param name="query">The query to be executed e.g "StormEvents | take 10"</param>
    /// <param name="clientRequestProperties">This Class contains additional information and properties for the query request</param>
    /// <param name="cancellationToken">A cancellation token for the request</param>
    /// <returns>IDataReader containing the results of the query</returns>
    Task<IDataReader> ExecuteQueryAsync(string queryUrl, string databaseItemId, string query, ClientRequestProperties clientRequestProperties, CancellationToken cancellationToken);

    /// <summary>
    /// Executes a control command against the Kusto database.
    /// </summary>
    /// <param name="queryUrl">The query url of the service e.g https://trd-uvbhpjmntkuc1b61qx.z1.kusto.fabric.microsoft.com</param>
    /// <param name="databaseItemId">The database Item id, alternatively can be the database display name for some cases e.g 4e6a777c-706f-4de4-a5c4-5b73c1ab2f5f </param>
    /// <param name="command">The command to be executed e.g ".show tables"</param>
    /// <param name="clientRequestProperties">This Class contains additional information and properties for the command request</param>
    /// <param name="cancellationToken">A cancellation token for the request</param>
    /// <returns>IDataReader containing the results of the command</returns>
    Task<IDataReader> ExecuteControlCommandAsync(string queryUrl, string databaseItemId, string command, ClientRequestProperties clientRequestProperties, CancellationToken cancellationToken);

    /// <summary>
    /// Ingests data as a stream into the Kusto database using a queued ingest client.
    /// </summary>
    /// <param name="ingestionUrl">The ingestion url of the service e.g https://ingest-trd-uvbhpjmntkuc1b61qx.z1.kusto.fabric.microsoft.com</param>
    /// <param name="stream">A stream of data to be ingested</param>
    /// <param name="ingestionProperties"> This Class contains additional information and properties for the ingestion request</param>
    /// <param name="token">The token to be used for authentication</param>
    /// <param name="sourceOptions">This Class contains additional information and properties for the source options</param>
    /// <returns>IKustoIngestionResult containing the results of the ingestion</returns>
    Task<IKustoIngestionResult> QueuedIngestFromStreamAsync(string ingestionUrl, Stream stream, KustoIngestionProperties ingestionProperties, string token, StreamSourceOptions sourceOptions = null);
    
    /// <summary>
    /// Ingests data as a stream into the Kusto database using a stream ingest client.
    /// </summary>
    /// <param name="ingestionUrl">The ingestion url of the service e.g https://ingest-trd-uvbhpjmntkuc1b61qx.z1.kusto.fabric.microsoft.com</param>
    /// <param name="stream">A stream of data to be ingested</param>
    /// <param name="ingestionProperties"> This Class contains additional information and properties for the ingestion request</param>
    /// <param name="token">The token to be used for authentication</param>
    /// <param name="sourceOptions">This Class contains additional information and properties for the source options</param>
    /// <returns>IKustoIngestionResult containing the results of the ingestion</returns>
    Task<IKustoIngestionResult> StreamIngestFromStreamAsync(string ingestionUrl, Stream stream, KustoIngestionProperties ingestionProperties, string token, StreamSourceOptions sourceOptions = null);
}