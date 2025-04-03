using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Fabric.Rti.workload.Backend.Services;

public interface IEventHubClient
{
    Task SendAsync(JArray jsonArray, CancellationToken cancellationToken = default);
}