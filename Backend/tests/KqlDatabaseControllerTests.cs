using System.Net;
using System.Text;
using System.Text.Json;
using Azure;
using Azure.Core;
using Azure.Core.Pipeline;
using Fabric.Rti.workload.Backend.Constants;
using Fabric.Rti.workload.Backend.Contracts;
using Fabric.Rti.workload.Backend.Contracts.RtiContracts;
using Fabric.Rti.workload.Backend.Controllers.Rti;
using Fabric.Rti.workload.Backend.Exceptions;
using Fabric.Rti.workload.Backend.Services;
using Kusto.Ingest;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Fabric.Api.KQLDatabase;
using Microsoft.Fabric.Api.KQLDatabase.Models;
using Moq;
using Xunit;

namespace Fabric.Rti.workload.Backend.Tests;

public partial class KqlDatabaseControllerTests
{
    private const string IngestionUrl = "https://ingest-sample.z1.kusto.fabric.microsoft.com";
    private const string FabricScope = $"{EnvironmentConstants.FabricBackendResourceId}/{WorkloadScopes.KQLDatabaseReadAll}";
    private readonly AuthorizationContext _authorizationContext = new();
    private readonly Mock<IAuthenticationService> _authentication = new(MockBehavior.Strict);
    private readonly Mock<IFabricApiClient> _fabric = new(MockBehavior.Strict);
    private readonly Mock<IKustoClientService> _kusto = new(MockBehavior.Strict);
    private readonly KqlDatabaseController _controller;
    private readonly KqlIngestRequest _request = new()
    {
        WorkspaceId = Guid.Parse("cfafbeb1-8037-4d0c-896e-a46fb27ff229"),
        KqlDatabaseItemId = Guid.Parse("35b3eb82-f902-4549-aad8-00f2c7a195be"),
        TableName = "IotData",
        IngestionMappingName = "SampleMapping",
        Content = "device-1,42"
    };

    public KqlDatabaseControllerTests()
    {
        var httpContext = new DefaultHttpContext();
        _controller = new KqlDatabaseController(NullLogger<KqlDatabaseController>.Instance,
            new HttpContextAccessor { HttpContext = httpContext }, _authentication.Object, _kusto.Object, _fabric.Object);
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        _authentication.Setup(service => service.AuthenticateDataPlaneCall(httpContext,
            It.Is<IList<string>>(scopes => scopes.SequenceEqual(new[] { WorkloadScopes.KQLDatabaseReadWriteAll }))))
            .ReturnsAsync(_authorizationContext);
        _authentication.Setup(service => service.GetAccessTokenOnBehalfOf(_authorizationContext,
            It.Is<IList<string>>(scopes => scopes.SequenceEqual(new[] { FabricScope })))).ReturnsAsync("fabric-token");
    }

    public static IEnumerable<object?[]> ValidEndpoints()
    {
        foreach (var endpoint in new[] { IngestionUrl, IngestionUrl + "/", IngestionUrl + ":443/", IngestionUrl.ToUpperInvariant() })
        {
            yield return new object?[] { true, endpoint };
            yield return new object?[] { false, endpoint };
        }
    }

    public static IEnumerable<object?[]> InvalidEndpoints()
    {
        foreach (var endpoint in new[]
        {
            null, "", "not-a-url", "/relative", "//localhost", "http://ingest-sample.z1.kusto.fabric.microsoft.com",
            "https://localhost", "https://127.0.0.1", "https://[::1]", "https://169.254.169.254",
            "https://attacker.example", "https://kusto.fabric.microsoft.com",
            "https://evilkusto.fabric.microsoft.com", "https://ingest-sample.z1.kusto.fabric.microsoft.com.attacker.example",
            IngestionUrl + ":8443", "https://user:password@ingest-sample.z1.kusto.fabric.microsoft.com",
            IngestionUrl + "/extra-path", IngestionUrl + "/%3BData%20Source=attacker",
            IngestionUrl + "/?key=value", IngestionUrl + "/#fragment",
            IngestionUrl + ";Data Source=https://attacker.example",
            "Data Source=" + IngestionUrl + ";User Token=attacker"
        })
        {
            yield return new object?[] { true, endpoint };
            yield return new object?[] { false, endpoint };
        }
    }

    [Theory]
    [MemberData(nameof(ValidEndpoints))]
    public async Task IngestionUsesAuthorizedFabricEndpoint(bool streaming, string endpoint)
    {
        var database = await CreateDatabaseAsync(endpoint);
        var databaseRetrieved = false;
        _fabric.Setup(service => service.GetKqlDatabaseAsync(_request.WorkspaceId, _request.KqlDatabaseItemId, "fabric-token"))
            .Callback(() => databaseRetrieved = true).ReturnsAsync(database);
        var expectedUri = new Uri(endpoint);
        var ingestionScope = $"{expectedUri.GetLeftPart(UriPartial.Authority)}/.default";
        _authentication.Setup(service => service.GetAccessTokenOnBehalfOf(_authorizationContext,
            It.Is<IList<string>>(scopes => scopes.SequenceEqual(new[] { ingestionScope }))))
            .Callback(() => Assert.True(databaseRetrieved)).ReturnsAsync("ingestion-token");
        var ingestionResult = new Mock<IKustoIngestionResult>();
        ingestionResult.Setup(result => result.GetIngestionStatusCollection()).Returns(new[] { new IngestionStatus { Status = Status.Succeeded } });

        Task<IKustoIngestionResult> Ingest(Uri uri, Stream stream, KustoIngestionProperties properties, string token, StreamSourceOptions options)
        {
            Assert.Equal(expectedUri, uri);
            Assert.Equal("ingestion-token", token);
            Assert.Equal(_request.KqlDatabaseItemId.ToString(), properties.DatabaseName);
            Assert.Equal(_request.TableName, properties.TableName);
            Assert.Equal(_request.IngestionMappingName, properties.IngestionMapping.IngestionMappingReference);
            Assert.Equal(Kusto.Data.Common.DataSourceFormat.csv, properties.Format);
            using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            Assert.Equal(_request.Content, reader.ReadToEnd());
            return Task.FromResult(ingestionResult.Object);
        }

        if (streaming)
        {
            _kusto.Setup(service => service.StreamIngestFromStreamAsync(It.IsAny<Uri>(), It.IsAny<Stream>(),
                It.IsAny<KustoIngestionProperties>(), It.IsAny<string>(), It.IsAny<StreamSourceOptions>())).Returns(Ingest);
        }
        else
        {
            _kusto.Setup(service => service.QueuedIngestFromStreamAsync(It.IsAny<Uri>(), It.IsAny<Stream>(),
                It.IsAny<KustoIngestionProperties>(), It.IsAny<string>(), It.IsAny<StreamSourceOptions>())).Returns(Ingest);
        }

        var requestJson = Newtonsoft.Json.JsonConvert.SerializeObject(new
        {
            _request.WorkspaceId,
            _request.KqlDatabaseItemId,
            _request.TableName,
            _request.IngestionMappingName,
            _request.Content,
            IngestionServiceUri = "https://attacker.example;Data Source=https://127.0.0.1"
        });
        var clientRequest = Newtonsoft.Json.JsonConvert.DeserializeObject<KqlIngestRequest>(requestJson)!;
        var actionResult = streaming ? await _controller.StreamingIngestToKqlDatabase(clientRequest) : await _controller.QueuedIngestToKqlDatabase(clientRequest);
        var result = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Same(ingestionResult.Object, result.Value);
        _fabric.VerifyAll();
        _kusto.VerifyAll();
        _authentication.Verify(service => service.GetAccessTokenOnBehalfOf(It.IsAny<AuthorizationContext>(), It.IsAny<IList<string>>()), Times.Exactly(2));
    }

    [Theory]
    [MemberData(nameof(InvalidEndpoints))]
    public async Task InvalidFabricEndpointIsRejectedBeforeIngestionTokenAcquisition(bool streaming, string? endpoint)
    {
        var database = await CreateDatabaseAsync(endpoint);
        _fabric.Setup(service => service.GetKqlDatabaseAsync(_request.WorkspaceId, _request.KqlDatabaseItemId, "fabric-token")).ReturnsAsync(database);

        Assert.Equal(StatusCodes.Status500InternalServerError, Assert.IsType<ObjectResult>(await IngestAsync(streaming)).StatusCode);
        VerifyNoKustoTokenOrRequest();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task MissingDatabaseMetadataFailsClosed(bool streaming)
    {
        _fabric.Setup(service => service.GetKqlDatabaseAsync(_request.WorkspaceId, _request.KqlDatabaseItemId, "fabric-token")).ReturnsAsync((KQLDatabase)null!);

        Assert.Equal(StatusCodes.Status500InternalServerError, Assert.IsType<ObjectResult>(await IngestAsync(streaming)).StatusCode);
        VerifyNoKustoTokenOrRequest();
    }

    [Theory]
    [InlineData(true, 401)]
    [InlineData(false, 401)]
    [InlineData(true, 403)]
    [InlineData(false, 403)]
    [InlineData(true, 404)]
    [InlineData(false, 404)]
    public async Task DatabaseAccessFailureStopsIngestion(bool streaming, int status)
    {
        _fabric.Setup(service => service.GetKqlDatabaseAsync(_request.WorkspaceId, _request.KqlDatabaseItemId, "fabric-token"))
            .ThrowsAsync(new RequestFailedException(status, "Database access denied or item not found."));

        Assert.Equal(status, Assert.IsType<StatusCodeResult>(await IngestAsync(streaming)).StatusCode);
        VerifyNoKustoTokenOrRequest();
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    public async Task EmptyIdentifiersAreRejectedWithoutExternalCalls(bool streaming, bool emptyWorkspace)
    {
        if (emptyWorkspace)
        {
            _request.WorkspaceId = Guid.Empty;
        }
        else
        {
            _request.KqlDatabaseItemId = Guid.Empty;
        }

        Assert.IsType<BadRequestObjectResult>(await IngestAsync(streaming));
        _authentication.VerifyNoOtherCalls();
        _fabric.VerifyNoOtherCalls();
        _kusto.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task InvalidModelStateIsRejectedWithoutExternalCalls(bool streaming)
    {
        _controller.ModelState.AddModelError(nameof(KqlIngestRequest.WorkspaceId), "Invalid UUID.");

        Assert.IsType<BadRequestObjectResult>(await IngestAsync(streaming));
        _authentication.VerifyNoOtherCalls();
        _fabric.VerifyNoOtherCalls();
        _kusto.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task NullRequestIsRejectedWithoutExternalCalls(bool streaming)
    {
        var result = streaming ? await _controller.StreamingIngestToKqlDatabase(null!) : await _controller.QueuedIngestToKqlDatabase(null!);

        Assert.IsType<BadRequestObjectResult>(result);
        _authentication.VerifyNoOtherCalls();
        _fabric.VerifyNoOtherCalls();
        _kusto.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AuthenticationFailureStopsTokenAcquisition(bool streaming)
    {
        _authentication.Setup(service => service.AuthenticateDataPlaneCall(It.IsAny<HttpContext>(), It.IsAny<IList<string>>()))
            .ThrowsAsync(new AuthenticationException("Caller lacks the required write scope."));

        Assert.IsType<UnauthorizedResult>(await IngestAsync(streaming));
        _authentication.Verify(service => service.GetAccessTokenOnBehalfOf(It.IsAny<AuthorizationContext>(), It.IsAny<IList<string>>()), Times.Never);
        _fabric.VerifyNoOtherCalls();
        _kusto.VerifyNoOtherCalls();
    }

    [Fact]
    public void RequestContractDoesNotAcceptAnIngestionEndpoint()
    {
        Assert.Null(typeof(KqlIngestRequest).GetProperty("IngestionServiceUri"));
        Assert.Equal(typeof(Guid), typeof(KqlIngestRequest).GetProperty(nameof(KqlIngestRequest.WorkspaceId))!.PropertyType);
        Assert.Equal(typeof(Guid), typeof(KqlIngestRequest).GetProperty(nameof(KqlIngestRequest.KqlDatabaseItemId))!.PropertyType);
    }

    private Task<IActionResult> IngestAsync(bool streaming)
    {
        return streaming ? _controller.StreamingIngestToKqlDatabase(_request) : _controller.QueuedIngestToKqlDatabase(_request);
    }

    private void VerifyNoKustoTokenOrRequest()
    {
        _authentication.Verify(service => service.GetAccessTokenOnBehalfOf(It.IsAny<AuthorizationContext>(),
            It.Is<IList<string>>(scopes => !scopes.SequenceEqual(new[] { FabricScope }))), Times.Never);
        _kusto.VerifyNoOtherCalls();
        _fabric.VerifyAll();
    }

    private async Task<KQLDatabase> CreateDatabaseAsync(string? endpoint, string? queryEndpoint = null)
    {
        var json = JsonSerializer.Serialize(new
        {
            id = _request.KqlDatabaseItemId,
            workspaceId = _request.WorkspaceId,
            displayName = "SampleDatabase",
            type = "KQLDatabase",
            properties = new { ingestionServiceUri = endpoint, queryServiceUri = queryEndpoint }
        });
        using var httpClient = new HttpClient(new JsonResponseHandler(json));
        var credential = new Mock<TokenCredential>();
        credential.Setup(token => token.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccessToken("fabric-token", DateTimeOffset.UtcNow.AddHours(1)));
        var options = new KQLDatabaseClientOptions { Transport = new HttpClientTransport(httpClient) };
        var client = new ItemsClient(credential.Object, new Uri(EnvironmentConstants.FabricApiBaseUrl), options);
        return (await client.GetKQLDatabaseAsync(_request.WorkspaceId, _request.KqlDatabaseItemId)).Value;
    }

    private sealed class JsonResponseHandler(string json) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") });
        }
    }
}
