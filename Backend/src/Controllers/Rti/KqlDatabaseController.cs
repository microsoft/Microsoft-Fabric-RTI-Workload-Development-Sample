using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fabric.Rti.workload.Backend.Constants;
using Fabric.Rti.workload.Backend.Contracts.RtiContracts;
using Fabric.Rti.workload.Backend.Exceptions;
using Fabric.Rti.workload.Backend.Services;
using Kusto.Data.Common;
using Kusto.Data.Data;
using Kusto.Ingest;
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
    
    [HttpPost("KqlDatabases/streamingIngest")]
    public async Task<IActionResult> StreamingIngestToKqlDatabase([FromBody] KqlIngestRequest request)
    {
        try
        {
            var authorizationContext = await _authenticationService.AuthenticateDataPlaneCall(
                _httpContextAccessor.HttpContext, allowedScopes: KqlDatabaseDataPlaneScopes);
            var scopes = new[] { $"{request.IngestionServiceUri}/.default" };

            var token = await _authenticationService.GetAccessTokenOnBehalfOf(authorizationContext, scopes);
            var ingestionProperties = CreateStreamingIngestionProperties(request);

            await using var stream = await GetStreamFromStringAsync(request.Content);
            
            var ingestionResult = await _kustoClientService.StreamIngestFromStreamAsync(request.IngestionServiceUri, stream, ingestionProperties, token);

            if (ingestionResult != null)
            {
                var ingestionStatus = ingestionResult.GetIngestionStatusCollection().ToList().First().Status;
                _logger.LogInformation($"StreamingIngestToKqlDatabase: Ingestion status: {ingestionStatus}");
            }
            
            return Ok(ingestionResult);
        }
        catch (AuthenticationException ex)
        {
            _logger.LogError($"StreamingIngestToKqlDatabase: Authentication failed for url {request.IngestionServiceUri}. Error: {ex.Message}");
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError($"StreamingIngestToKqlDatabase: failed ingesting to {request.IngestionServiceUri} Error: {ex.Message}");
            return Problem();
        }
    }

    [HttpPost("KqlDatabases/queuedIngest")]
    public async Task<IActionResult> QueuedIngestToKqlDatabase([FromBody] KqlIngestRequest request)
    {
        try
        {
            var authorizationContext = await _authenticationService.AuthenticateDataPlaneCall(
                _httpContextAccessor.HttpContext, allowedScopes: KqlDatabaseDataPlaneScopes);
            var scopes = new[] { $"{request.IngestionServiceUri}/.default" };

            var token = await _authenticationService.GetAccessTokenOnBehalfOf(authorizationContext, scopes);
            var ingestionProperties = CreateQueuedIngestionProperties(request);

            await using var stream = await GetStreamFromStringAsync(request.Content);
            
            var ingestionResult = await _kustoClientService.QueuedIngestFromStreamAsync(request.IngestionServiceUri, stream, ingestionProperties, token);

            if (ingestionResult != null)
            {
                var ingestionStatus = ingestionResult.GetIngestionStatusCollection().ToList().First().Status;
                _logger.LogInformation($"QueuedIngestToKqlDatabase: Ingestion status: {ingestionStatus}");
            }
            
            return Ok(ingestionResult);
        }
        catch (AuthenticationException ex)
        {
            _logger.LogError($"QueuedIngestToKqlDatabase: Authentication failed for url {request.IngestionServiceUri}. Error: {ex.Message}");
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError($"QueuedIngestToKqlDatabase: failed ingesting to {request.IngestionServiceUri} Error: {ex.Message}");
            return Problem();
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
    
    private KustoIngestionProperties CreateStreamingIngestionProperties(KqlIngestRequest request)
    {
        var ingestProps = new KustoIngestionProperties(request.KqlDatabaseItemId, request.TableName)
        {
            IngestionMapping =
            {
                IngestionMappingReference = request.IngestionMappingName
            },
            // set to false, assuming the content is provided without a header and the first line is a record 
            AdditionalProperties = new Dictionary<string, string> { { "ignoreFirstRecord", "False" } },
            Format = DataSourceFormat.csv
        };

        return ingestProps;
    }
    
    private KustoQueuedIngestionProperties CreateQueuedIngestionProperties(KqlIngestRequest request)
    {
        var ingestProps = new KustoQueuedIngestionProperties(request.KqlDatabaseItemId, request.TableName)
        {
            ReportLevel = IngestionReportLevel.FailuresAndSuccesses,
            ReportMethod = IngestionReportMethod.Queue,
            IngestionMapping =
            {
                IngestionMappingReference = request.IngestionMappingName
            },
            // set to false, assuming the content is provided without a header and the first line is a record 
            AdditionalProperties = new Dictionary<string, string> { { "ignoreFirstRecord", "False" } },
            Format = DataSourceFormat.csv
        };

        return ingestProps;
    }

    private async Task<Stream> GetStreamFromStringAsync(string content)
    {
        var memoryStream = new MemoryStream(Encoding.UTF8.GetByteCount(content));
        await using var writer = new StreamWriter(memoryStream, Encoding.UTF8, -1, true);
        
        await writer.WriteAsync(content);
        await writer.FlushAsync();
        memoryStream.Position = 0;

        return memoryStream;
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