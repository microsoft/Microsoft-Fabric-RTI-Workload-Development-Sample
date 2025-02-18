using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fabric.Rti.workload.Backend.Constants;
using Fabric.Rti.workload.Backend.Contracts.RtiContracts;
using Fabric.Rti.workload.Backend.Exceptions;
using Fabric.Rti.workload.Backend.Services;
using Kusto.Data.Common;
using Kusto.Data.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Fabric.Rti.workload.Backend.Controllers.Rti;

public class KqlDatabaseController : ControllerBase
{
    private static readonly TimeSpan DefaultQueryTimeout = TimeSpan.FromSeconds(30);
    private static readonly IList<string> KqlDatabaseDataPlaneScopes = new[] { WorkloadScopes.KQLDatabaseReadAll, WorkloadScopes.KQLDatabaseReadWriteAll };

    private readonly ILogger<KqlDatabaseController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthenticationService _authenticationService;
    private readonly IKustoClientService _kustoClientService;
    private readonly IHttpClientService _httpClientService;

    public KqlDatabaseController(
        ILogger<KqlDatabaseController> logger,
        IHttpContextAccessor httpContextAccessor,
        IAuthenticationService authenticationService,
        IKustoClientService kustoClientService)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _authenticationService = authenticationService;
        _kustoClientService = kustoClientService;
    }

    [HttpPost("KqlDatabases/query")]
    public async Task<IActionResult> ExecuteKqlQuery([FromBody] KqlQueryRequest request)
    {
        try
        {
            var authorizationContext = await _authenticationService.AuthenticateDataPlaneCall(
                _httpContextAccessor.HttpContext, allowedScopes: KqlDatabaseDataPlaneScopes);
            var scopes = new[] { $"{request.KqlDatabaseQueryUrl}/.default" };
            var token = await _authenticationService.GetAccessTokenOnBehalfOf(authorizationContext, scopes);

            var clientRequestProperties = GenerateClientRequestProperties(token);
            var dataReader = await _kustoClientService.ExecuteQueryAsync(
                request.KqlDatabaseQueryUrl, request.KqlDatabaseItemId, request.Query, clientRequestProperties, default);

            var stream = KustoJsonDataStream.GetReaderDataAsStream(dataReader);
            return Ok(stream);
        }
        catch (AuthenticationException ex)
        {
            _logger.LogError($"ExecuteKqlQuery: Authentication failed for url {request.KqlDatabaseQueryUrl}. Error: {ex.Message}");
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError($"ExecuteKqlQuery: Error executing KQL query for url {request.KqlDatabaseQueryUrl}. Error: {ex.Message}");
            return BadRequest();
        }
    }

    [HttpPost("KqlDatabases/mgmt")]
    public async Task<IActionResult> ExecuteKqlManagementCommand([FromBody] KqlManagementCommandRequest request)
    {
        try
        {
            var authorizationContext = await _authenticationService.AuthenticateDataPlaneCall(
                _httpContextAccessor.HttpContext, allowedScopes: KqlDatabaseDataPlaneScopes);
            var scopes = new[] { $"{request.KqlDatabaseQueryUrl}/.default" };
            var token = await _authenticationService.GetAccessTokenOnBehalfOf(authorizationContext, scopes);

            var clientRequestProperties = GenerateClientRequestProperties(token);
            var dataReader = await _kustoClientService.ExecuteControlCommandAsync(
                request.KqlDatabaseQueryUrl, request.KqlDatabaseItemId, request.Command, clientRequestProperties, default);

            var stream = KustoJsonDataStream.GetReaderDataAsStream(dataReader);
            return Ok(stream);
        }
        catch (AuthenticationException ex)
        {
            _logger.LogError($"ExecuteKqlQuery: Authentication failed for url {request.KqlDatabaseQueryUrl}. Error: {ex.Message}");
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError($"ExecuteKqlQuery: Error executing KQL query for url {request.KqlDatabaseQueryUrl}. Error: {ex.Message}");
            return BadRequest();
        }
    }

    private ClientRequestProperties GenerateClientRequestProperties(string token)
    {
        var properties = new ClientRequestProperties
        {
            ClientRequestId = GetRequestIdHeader() ?? Guid.NewGuid().ToString(),
            AuthorizationScheme = "Bearer",
            SecurityToken = token
        };

        properties.SetOption(ClientRequestProperties.OptionServerTimeout, DefaultQueryTimeout);
        return properties;
    }

    private string GetRequestIdHeader()
    {
        if (_httpContextAccessor.HttpContext != null && _httpContextAccessor.HttpContext.Request.Headers.TryGetValue(HttpHeaders.RequestId, out var headerValue))
        {
            return headerValue;
        }

        return null;
    }
}