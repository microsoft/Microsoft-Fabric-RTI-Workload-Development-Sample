using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Kusto.Data.Common;

namespace Fabric.Rti.workload.Backend.Services;

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
    /// <returns></returns>
    Task<IDataReader> ExecuteQueryAsync(string queryUrl, string databaseItemId, string query, ClientRequestProperties clientRequestProperties, CancellationToken cancellationToken);

    /// <summary>
    /// Executes a control command against the Kusto database.
    /// </summary>
    /// <param name="queryUrl">The query url of the service e.g https://trd-uvbhpjmntkuc1b61qx.z1.kusto.fabric.microsoft.com</param>
    /// <param name="databaseItemId">The database Item id, alternatively can be the database display name for some cases e.g 4e6a777c-706f-4de4-a5c4-5b73c1ab2f5f </param>
    /// <param name="command">The command to be executed e.g ".show tables"</param>
    /// <param name="clientRequestProperties">This Class contains additional information and properties for the command request</param>
    /// <param name="cancellationToken">A cancellation token for the request</param>
    /// <returns></returns>
    Task<IDataReader> ExecuteControlCommandAsync(string queryUrl, string databaseItemId, string command, ClientRequestProperties clientRequestProperties, CancellationToken cancellationToken);
}