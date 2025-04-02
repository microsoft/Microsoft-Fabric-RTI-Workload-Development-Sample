using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fabric.Rti.workload.Backend.Constants;
using Fabric.Rti.workload.Backend.Contracts.RtiContracts;
using Fabric.Rti.workload.Backend.Exceptions;
using Fabric.Rti.workload.Backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Fabric.Rti.workload.Backend.Controllers.Rti;

public class EventstreamController : ControllerBase
{
    private static readonly IList<string> EventstreamFabricScopes = new[] { $"{EnvironmentConstants.FabricBackendResourceId}/Eventstream.ReadWrite.All" };

    private readonly ILogger<KqlDatabaseController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthenticationService _authenticationService;
    private readonly IFabricApiClient _fabricApiClient;

    public EventstreamController(
        ILogger<KqlDatabaseController> logger,
        IHttpContextAccessor httpContextAccessor,
        IAuthenticationService authenticationService,
        IFabricApiClient fabricApiClient)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _authenticationService = authenticationService;
        _fabricApiClient = fabricApiClient;
    }

    [HttpPost("workspaces/{workspaceId}/eventstreams/{eventstreamId}/sendEvents")]
    public async Task<IActionResult> SendEvents(Guid workspaceId, Guid eventstreamId, [FromBody] EventstreamSendEventsRequest request)
    {
        try
        {
            var authorizationContext = await _authenticationService.AuthenticateDataPlaneCall(
                _httpContextAccessor.HttpContext, allowedScopes: new[] { WorkloadScopes.FabricEventstreamReadWriteAll });
            var token = await _authenticationService.GetAccessTokenOnBehalfOf(authorizationContext, EventstreamFabricScopes);

            await using var eventHubClient = await CreateEventHubClient(workspaceId, eventstreamId, token);
            var jsonArray = request.Events;
            await eventHubClient.SendAsync(jsonArray);

            return Ok();
        }
        catch (AuthenticationException ex)
        {
            _logger.LogError($"SendEvents: Authentication failed for EventStream Item {eventstreamId}. Error: {ex.Message}");
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError($"SendEvents: Error sending events to EventStream Item {eventstreamId}. Error: {ex.Message}");
            return BadRequest();
        }
    }

    private async Task<EventHubClient> CreateEventHubClient(Guid workspaceId, Guid eventStreamItemId, string fabricToken)
    {
        var connectionString = await GetEventStreamCustomEndpointConnectionString(workspaceId, eventStreamItemId, fabricToken);
     
        return new EventHubClient(connectionString);
    }

    private async Task<string> GetEventStreamCustomEndpointConnectionString(Guid workspaceId, Guid eventStreamItemId, string fabricToken)
    {
        var eventstreamTopology = await _fabricApiClient.GetEventstreamTopologyAsync(workspaceId, eventStreamItemId, fabricToken);
        var customEndpointSourceId =
            eventstreamTopology.Sources
                .Where(source => string.Equals(source.Name, RtiConstants.EventStreamCustomEndpointSourceName, StringComparison.OrdinalIgnoreCase))
                .Select(source => source.Id).FirstOrDefault();

        if (customEndpointSourceId == null)
        {
            throw new InvalidOperationException($"Custom endpoint source '{RtiConstants.EventStreamCustomEndpointSourceName}' not found in event stream topology.");
        }

        var customEndpointConnection = await _fabricApiClient.GetEventstreamSourceConnectionAsync(workspaceId, eventStreamItemId, Guid.Parse(customEndpointSourceId), fabricToken);

        if (customEndpointConnection == null)
        {
            throw new InvalidOperationException($"Custom endpoint connection for source '{RtiConstants.EventStreamCustomEndpointSourceName}' not found.");
        }

        var connectionString = customEndpointConnection.AccessKeys.PrimaryConnectionString;

        return connectionString;
    }
}
    