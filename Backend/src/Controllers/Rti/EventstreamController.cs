using System;
using System.Collections.Generic;
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
    private static readonly IList<string> EventstreamFabricScopes = new[] { $"{EnvironmentConstants.FabricBackendResourceId}/{WorkloadScopes.EventstreamReadWriteAll}" };
    
    private readonly ILogger<KqlDatabaseController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthenticationService _authenticationService;
    
    public EventstreamController(
        ILogger<KqlDatabaseController> logger,
        IHttpContextAccessor httpContextAccessor,
        IAuthenticationService authenticationService)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _authenticationService = authenticationService;
    }
    
    [HttpPost("Eventstream/{eventStreamItemId}/Send")]
    public async Task<IActionResult> SendEventStreamItem(string eventStreamItemId, [FromBody] EventstreamSendEventsRequest request)
    {
        try
        {
            var authorizationContext = await _authenticationService.AuthenticateDataPlaneCall(
                _httpContextAccessor.HttpContext, allowedScopes: new[] {WorkloadScopes.EventstreamReadWriteAll});
            var token = await _authenticationService.GetAccessTokenOnBehalfOf(authorizationContext, EventstreamFabricScopes);
            
            await using var eventHubClient = CreateEventHubClient(eventStreamItemId, token);
            var jsonArray = request.Events;
            await eventHubClient.SendAsync(jsonArray);

            return Ok();
        }
        catch (AuthenticationException ex)
        {
            _logger.LogError($"SendEventStreamItem: Authentication failed for EventStream Item {eventStreamItemId}. Error: {ex.Message}");
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError($"ExecuteKqlQuery: Error sending events to EventStream Item {eventStreamItemId}. Error: {ex.Message}");
            return BadRequest();
        }
    }
    
    private EventHubClient CreateEventHubClient(string eventStreamItemId, string fabricToken)
    {
        var connectionString = "DummyConnectionString"; // Replace with actual connection string retrieval logic
        return new EventHubClient(connectionString);
    }
}