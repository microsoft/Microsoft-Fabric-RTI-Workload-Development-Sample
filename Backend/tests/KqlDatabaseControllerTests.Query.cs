using System.Data;
using Azure;
using Fabric.Rti.workload.Backend.Constants;
using Fabric.Rti.workload.Backend.Contracts;
using Fabric.Rti.workload.Backend.Contracts.RtiContracts;
using Fabric.Rti.workload.Backend.Exceptions;
using Kusto.Data.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Fabric.Api.KQLDatabase.Models;
using Moq;
using Xunit;

namespace Fabric.Rti.workload.Backend.Tests;

public partial class KqlDatabaseControllerTests
{
    private const string QueryUrl = "https://sample.z1.kusto.fabric.microsoft.com";

    public static IEnumerable<object?[]> ValidQueryEndpoints()
    {
        foreach (var endpoint in new[] { QueryUrl, QueryUrl + "/", QueryUrl + ":443/", QueryUrl.ToUpperInvariant() })
        {
            yield return new object?[] { true, endpoint };
            yield return new object?[] { false, endpoint };
        }
    }

    [Theory]
    [MemberData(nameof(ValidQueryEndpoints))]
    public async Task QueryOrCommandUsesAuthorizedFabricEndpoint(bool management, string endpoint)
    {
        SetupQueryAuthentication();
        _controller.HttpContext.Request.Headers[HttpHeaders.RequestId] = "query-request-id";
        var database = await CreateDatabaseAsync(IngestionUrl, endpoint);
        var databaseRetrieved = false;
        _fabric.Setup(service => service.GetKqlDatabaseAsync(_request.WorkspaceId, _request.KqlDatabaseItemId, "fabric-token"))
            .Callback(() => databaseRetrieved = true).ReturnsAsync(database);
        var expectedUri = new Uri(endpoint);
        var scope = $"{expectedUri.GetLeftPart(UriPartial.Authority)}/.default";
        _authentication.Setup(service => service.GetAccessTokenOnBehalfOf(_authorizationContext,
            It.Is<IList<string>>(scopes => scopes.SequenceEqual(new[] { scope }))))
            .Callback(() => Assert.True(databaseRetrieved)).ReturnsAsync("query-token");

        using var table = new DataTable();
        table.Columns.Add("DeviceId", typeof(string));
        table.Columns.Add("Value", typeof(int));
        table.Rows.Add("device-1", 42);
        using var dataReader = table.CreateDataReader();

        void Execute(string url, string databaseId, string text, ClientRequestProperties properties, CancellationToken cancellationToken)
        {
            Assert.Equal(expectedUri.AbsoluteUri, url);
            Assert.Equal(_request.KqlDatabaseItemId.ToString(), databaseId);
            Assert.Equal(management ? ".show tables" : "IotData | take 10", text);
            Assert.Equal("query-token", properties.SecurityToken);
            Assert.Equal("Bearer", properties.AuthorizationScheme);
            Assert.Equal("query-request-id", properties.ClientRequestId);
            Assert.Equal(TimeSpan.FromSeconds(30), properties.GetOptionValueOrDefault(ClientRequestProperties.OptionServerTimeout, TimeSpan.Zero));
            Assert.Equal(CancellationToken.None, cancellationToken);
        }

        if (management)
        {
            _kusto.Setup(service => service.ExecuteControlCommandAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<ClientRequestProperties>(), It.IsAny<CancellationToken>())).Callback(Execute).ReturnsAsync(dataReader);
        }
        else
        {
            _kusto.Setup(service => service.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<ClientRequestProperties>(), It.IsAny<CancellationToken>())).Callback(Execute).ReturnsAsync(dataReader);
        }

        var requestJson = Newtonsoft.Json.JsonConvert.SerializeObject(new
        {
            _request.WorkspaceId,
            _request.KqlDatabaseItemId,
            Query = "IotData | take 10",
            Command = ".show tables",
            KqlDatabaseQueryUrl = "https://attacker.example;Data Source=https://127.0.0.1",
            QueryServiceUri = "https://169.254.169.254"
        });
        var actionResult = management
            ? await _controller.ExecuteKqlManagementCommand(Newtonsoft.Json.JsonConvert.DeserializeObject<KqlManagementCommandRequest>(requestJson)!)
            : await _controller.ExecuteKqlQuery(Newtonsoft.Json.JsonConvert.DeserializeObject<KqlQueryRequest>(requestJson)!);
        var result = Assert.IsType<OkObjectResult>(actionResult);
        using var stream = Assert.IsAssignableFrom<Stream>(result.Value);
        using var streamReader = new StreamReader(stream);
        var response = await streamReader.ReadToEndAsync(TestContext.Current.CancellationToken);
        Assert.Contains("device-1", response);
        Assert.Contains("DeviceId", response);
        Assert.Contains("42", response);
        _fabric.VerifyAll();
        _kusto.VerifyAll();
        _authentication.Verify(service => service.AuthenticateDataPlaneCall(It.IsAny<HttpContext>(),
            It.Is<IList<string>>(scopes => scopes.SequenceEqual(new[] { WorkloadScopes.KQLDatabaseReadAll, WorkloadScopes.KQLDatabaseReadWriteAll }))), Times.Once);
        _authentication.Verify(service => service.GetAccessTokenOnBehalfOf(It.IsAny<AuthorizationContext>(), It.IsAny<IList<string>>()), Times.Exactly(2));
    }

    [Theory]
    [MemberData(nameof(InvalidEndpoints))]
    public async Task InvalidQueryEndpointIsRejectedBeforeKustoTokenAcquisition(bool management, string? endpoint)
    {
        SetupQueryAuthentication();
        var database = await CreateDatabaseAsync(IngestionUrl, endpoint);
        _fabric.Setup(service => service.GetKqlDatabaseAsync(_request.WorkspaceId, _request.KqlDatabaseItemId, "fabric-token")).ReturnsAsync(database);

        Assert.IsType<BadRequestResult>(await QueryOrCommandAsync(management));
        VerifyNoKustoTokenOrRequest();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task QueryWithoutDatabaseMetadataFailsClosed(bool management)
    {
        SetupQueryAuthentication();
        _fabric.Setup(service => service.GetKqlDatabaseAsync(_request.WorkspaceId, _request.KqlDatabaseItemId, "fabric-token")).ReturnsAsync((KQLDatabase)null!);

        Assert.IsType<BadRequestResult>(await QueryOrCommandAsync(management));
        VerifyNoKustoTokenOrRequest();
    }

    [Theory]
    [InlineData(true, 401)]
    [InlineData(false, 401)]
    [InlineData(true, 403)]
    [InlineData(false, 403)]
    [InlineData(true, 404)]
    [InlineData(false, 404)]
    public async Task DatabaseAccessFailureStopsQueriesAndCommands(bool management, int status)
    {
        SetupQueryAuthentication();
        _fabric.Setup(service => service.GetKqlDatabaseAsync(_request.WorkspaceId, _request.KqlDatabaseItemId, "fabric-token"))
            .ThrowsAsync(new RequestFailedException(status, "Database access denied or item not found."));

        Assert.Equal(status, Assert.IsType<StatusCodeResult>(await QueryOrCommandAsync(management)).StatusCode);
        VerifyNoKustoTokenOrRequest();
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    public async Task QueryWithEmptyIdentifiersIsRejectedWithoutExternalCalls(bool management, bool emptyWorkspace)
    {
        if (emptyWorkspace)
        {
            _request.WorkspaceId = Guid.Empty;
        }
        else
        {
            _request.KqlDatabaseItemId = Guid.Empty;
        }

        Assert.IsType<BadRequestObjectResult>(await QueryOrCommandAsync(management));
        _authentication.VerifyNoOtherCalls();
        _fabric.VerifyNoOtherCalls();
        _kusto.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task QueryWithInvalidModelStateIsRejectedWithoutExternalCalls(bool management)
    {
        _controller.ModelState.AddModelError(nameof(KqlQueryRequest.WorkspaceId), "Invalid UUID.");

        Assert.IsType<BadRequestObjectResult>(await QueryOrCommandAsync(management));
        _authentication.VerifyNoOtherCalls();
        _fabric.VerifyNoOtherCalls();
        _kusto.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task NullQueryRequestIsRejectedWithoutExternalCalls(bool management)
    {
        var result = management ? await _controller.ExecuteKqlManagementCommand(null!) : await _controller.ExecuteKqlQuery(null!);

        Assert.IsType<BadRequestObjectResult>(result);
        _authentication.VerifyNoOtherCalls();
        _fabric.VerifyNoOtherCalls();
        _kusto.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task QueryAuthenticationFailureStopsTokenAcquisition(bool management)
    {
        _authentication.Setup(service => service.AuthenticateDataPlaneCall(It.IsAny<HttpContext>(), It.IsAny<IList<string>>()))
            .ThrowsAsync(new AuthenticationException("Caller lacks a required query scope."));

        Assert.IsType<UnauthorizedResult>(await QueryOrCommandAsync(management));
        _authentication.Verify(service => service.GetAccessTokenOnBehalfOf(It.IsAny<AuthorizationContext>(), It.IsAny<IList<string>>()), Times.Never);
        _fabric.VerifyNoOtherCalls();
        _kusto.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void QueryContractsDoNotAcceptClientEndpoints(bool management)
    {
        var requestType = management ? typeof(KqlManagementCommandRequest) : typeof(KqlQueryRequest);
        Assert.Null(requestType.GetProperty("KqlDatabaseQueryUrl"));
        Assert.Equal(typeof(Guid), requestType.GetProperty(nameof(KqlQueryRequest.WorkspaceId))!.PropertyType);
        Assert.Equal(typeof(Guid), requestType.GetProperty(nameof(KqlQueryRequest.KqlDatabaseItemId))!.PropertyType);
    }

    private void SetupQueryAuthentication()
    {
        _authentication.Setup(service => service.AuthenticateDataPlaneCall(It.IsAny<HttpContext>(),
            It.Is<IList<string>>(scopes => scopes.SequenceEqual(new[] { WorkloadScopes.KQLDatabaseReadAll, WorkloadScopes.KQLDatabaseReadWriteAll }))))
            .ReturnsAsync(_authorizationContext);
    }

    private Task<IActionResult> QueryOrCommandAsync(bool management)
    {
        return management
            ? _controller.ExecuteKqlManagementCommand(new KqlManagementCommandRequest
            {
                WorkspaceId = _request.WorkspaceId, KqlDatabaseItemId = _request.KqlDatabaseItemId, Command = ".show tables"
            })
            : _controller.ExecuteKqlQuery(new KqlQueryRequest
            {
                WorkspaceId = _request.WorkspaceId, KqlDatabaseItemId = _request.KqlDatabaseItemId, Query = "IotData | take 10"
            });
    }
}
