using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Fabric.Rti.workload.Backend.Constants;
using Fabric.Rti.workload.Backend.Contracts;
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
    private readonly IFabricApiClient _fabricApiClient;

    public KqlDatabaseController(
        ILogger<KqlDatabaseController> logger,
        IHttpContextAccessor httpContextAccessor,
        IAuthenticationService authenticationService,
        IKustoClientService kustoClientService,
        IFabricApiClient fabricApiClient)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _authenticationService = authenticationService;
        _kustoClientService = kustoClientService;
        _fabricApiClient = fabricApiClient;
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
        if (!ModelState.IsValid || request == null || request.WorkspaceId == Guid.Empty || request.KqlDatabaseItemId == Guid.Empty)
        {
            _logger.LogWarning("StreamingIngestToKqlDatabase: Missing or invalid workspace or KQL database ID.");
            return BadRequest("Valid workspace and KQL database IDs are required.");
        }

        try
        {
            var authorizationContext = await _authenticationService.AuthenticateDataPlaneCall(
                _httpContextAccessor.HttpContext, allowedScopes: new[] { WorkloadScopes.KQLDatabaseReadWriteAll });
            var ingestionUri = await GetIngestionServiceUriAsync(request, authorizationContext);
            var scopes = new[] { $"{ingestionUri.GetLeftPart(UriPartial.Authority)}/.default" };

            var token = await _authenticationService.GetAccessTokenOnBehalfOf(authorizationContext, scopes);
            var ingestionProperties = CreateStreamingIngestionProperties(request);

            await using var stream = await GetStreamFromStringAsync(request.Content);
            
            var ingestionResult = await _kustoClientService.StreamIngestFromStreamAsync(ingestionUri, stream, ingestionProperties, token);

            if (ingestionResult != null)
            {
                var ingestionStatus = ingestionResult.GetIngestionStatusCollection().ToList().First().Status;
                _logger.LogInformation($"StreamingIngestToKqlDatabase: Ingestion status: {ingestionStatus}");
            }
            
            return Ok(ingestionResult);
        }
        catch (AuthenticationException ex)
        {
            _logger.LogError($"StreamingIngestToKqlDatabase: Authentication failed for database {request.KqlDatabaseItemId} in workspace {request.WorkspaceId}. Error: {ex.Message}");
            return Unauthorized();
        }
        catch (RequestFailedException ex) when (ex.Status == StatusCodes.Status401Unauthorized ||
            ex.Status == StatusCodes.Status403Forbidden || ex.Status == StatusCodes.Status404NotFound)
        {
            _logger.LogWarning($"StreamingIngestToKqlDatabase: Database access failed for {request.KqlDatabaseItemId} in workspace {request.WorkspaceId}. Error: {ex.Message}");
            return StatusCode(ex.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError($"StreamingIngestToKqlDatabase: Failed ingesting to database {request.KqlDatabaseItemId} in workspace {request.WorkspaceId}. Error: {ex.Message}");
            return Problem();
        }
    }

    [HttpPost("KqlDatabases/queuedIngest")]
    public async Task<IActionResult> QueuedIngestToKqlDatabase([FromBody] KqlIngestRequest request)
    {
        if (!ModelState.IsValid || request == null || request.WorkspaceId == Guid.Empty || request.KqlDatabaseItemId == Guid.Empty)
        {
            _logger.LogWarning("QueuedIngestToKqlDatabase: Missing or invalid workspace or KQL database ID.");
            return BadRequest("Valid workspace and KQL database IDs are required.");
        }

        try
        {
            var authorizationContext = await _authenticationService.AuthenticateDataPlaneCall(
                _httpContextAccessor.HttpContext, allowedScopes: new[] { WorkloadScopes.KQLDatabaseReadWriteAll });
            var ingestionUri = await GetIngestionServiceUriAsync(request, authorizationContext);
            var scopes = new[] { $"{ingestionUri.GetLeftPart(UriPartial.Authority)}/.default" };

            var token = await _authenticationService.GetAccessTokenOnBehalfOf(authorizationContext, scopes);
            var ingestionProperties = CreateQueuedIngestionProperties(request);

            await using var stream = await GetStreamFromStringAsync(request.Content);
            
            var ingestionResult = await _kustoClientService.QueuedIngestFromStreamAsync(ingestionUri, stream, ingestionProperties, token);

            if (ingestionResult != null)
            {
                var ingestionStatus = ingestionResult.GetIngestionStatusCollection().ToList().First().Status;
                _logger.LogInformation($"QueuedIngestToKqlDatabase: Ingestion status: {ingestionStatus}");
            }
            
            return Ok(ingestionResult);
        }
        catch (AuthenticationException ex)
        {
            _logger.LogError($"QueuedIngestToKqlDatabase: Authentication failed for database {request.KqlDatabaseItemId} in workspace {request.WorkspaceId}. Error: {ex.Message}");
            return Unauthorized();
        }
        catch (RequestFailedException ex) when (ex.Status == StatusCodes.Status401Unauthorized ||
            ex.Status == StatusCodes.Status403Forbidden || ex.Status == StatusCodes.Status404NotFound)
        {
            _logger.LogWarning($"QueuedIngestToKqlDatabase: Database access failed for {request.KqlDatabaseItemId} in workspace {request.WorkspaceId}. Error: {ex.Message}");
            return StatusCode(ex.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError($"QueuedIngestToKqlDatabase: Failed ingesting to database {request.KqlDatabaseItemId} in workspace {request.WorkspaceId}. Error: {ex.Message}");
            return Problem();
        }
    }

    private async Task<Uri> GetIngestionServiceUriAsync(KqlIngestRequest request, AuthorizationContext authorizationContext)
    {
        var fabricScopes = new[] { $"{EnvironmentConstants.FabricBackendResourceId}/{WorkloadScopes.KQLDatabaseReadAll}" };
        var fabricToken = await _authenticationService.GetAccessTokenOnBehalfOf(authorizationContext, fabricScopes);
        var kqlDatabase = await _fabricApiClient.GetKqlDatabaseAsync(request.WorkspaceId, request.KqlDatabaseItemId, fabricToken);
        var ingestionUrl = kqlDatabase?.Properties?.IngestionServiceUri;

        if (!Uri.TryCreate(ingestionUrl, UriKind.Absolute, out var ingestionUri) ||
            ingestionUri.Scheme != Uri.UriSchemeHttps || ingestionUri.HostNameType != UriHostNameType.Dns ||
            !ingestionUri.Host.EndsWith(".kusto.fabric.microsoft.com", StringComparison.OrdinalIgnoreCase) ||
            !ingestionUri.IsDefaultPort || ingestionUri.UserInfo.Length != 0 || ingestionUri.AbsolutePath != "/" ||
            ingestionUri.Query.Length != 0 || ingestionUri.Fragment.Length != 0)
        {
            throw new InvalidOperationException("Fabric returned an invalid KQL database ingestion endpoint.");
        }

        return ingestionUri;
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
        var ingestProps = new KustoIngestionProperties(request.KqlDatabaseItemId.ToString(), request.TableName)
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
        var ingestProps = new KustoQueuedIngestionProperties(request.KqlDatabaseItemId.ToString(), request.TableName)
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