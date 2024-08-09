using System.Net;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NSubstitute;
using Utconnect.Coffer.Models;
using Utconnect.Coffer.Services.Implementations;
using Utconnect.Common.Models;

namespace Coffer.Test.Services;

public class CofferServiceTests
{
    private readonly IHttpClientFactory _mockHttpClientFactory;
    private readonly IOptions<CofferConfig> _mockConfig;
    private readonly CofferService _cofferService;

    public CofferServiceTests()
    {
        _mockHttpClientFactory = Substitute.For<IHttpClientFactory>();
        _mockConfig = Substitute.For<IOptions<CofferConfig>>();
        _cofferService = new CofferService(_mockHttpClientFactory, _mockConfig);
    }

    [Fact]
    public async Task GetKey_ShouldReturnFailure_WhenCofferUrlIsEmpty()
    {
        // Arrange
        _mockConfig.Value.Returns(new CofferConfig { Url = string.Empty });

        // Act
        Result<string> result = await _cofferService.GetKey("app", "secretName");

        // Assert
        Assert.False(result.Success);
        Assert.Single(result.Errors!);
        Assert.Equal(HttpStatusCode.InternalServerError, result.Errors?.First().Code);
        Assert.Equal("Coffer URL is empty", result.Errors?.First().Message);
    }

    [Fact]
    public async Task GetKey_ShouldReturnFailure_WhenResponseIsNotSuccess()
    {
        // Arrange
        CofferConfig config = new() { Url = "http://coffer-service" };
        _mockConfig.Value.Returns(config);

        HttpMessageHandlerMock mockHttpMessageHandler = new(HttpStatusCode.InternalServerError);
        HttpClient client = new(mockHttpMessageHandler);
        _mockHttpClientFactory.CreateClient().Returns(client);

        // Act
        Result<string> result = await _cofferService.GetKey("app", "secretName");

        // Assert
        Assert.False(result.Success);
        Assert.Single(result.Errors!);
        Assert.Equal(HttpStatusCode.InternalServerError, result.Errors?.First().Code);
        Assert.Equal("Response status is not success", result.Errors?.First().Message);
    }

    [Fact]
    public async Task GetKey_ShouldReturnFailure_WhenResponseContentIsNull()
    {
        // Arrange
        CofferConfig config = new() { Url = "http://coffer-service" };
        _mockConfig.Value.Returns(config);

        HttpMessageHandlerMock mockHttpMessageHandler = new(HttpStatusCode.OK, "null");
        HttpClient client = new(mockHttpMessageHandler);
        _mockHttpClientFactory.CreateClient().Returns(client);

        // Act
        Result<string> result = await _cofferService.GetKey("app", "secretName");

        // Assert
        Assert.False(result.Success);
        Assert.Single(result.Errors!);
        Assert.Equal(HttpStatusCode.InternalServerError, result.Errors?.First().Code);
        Assert.Equal("Retrieved data is null", result.Errors?.First().Message);
    }

    [Fact]
    public async Task GetKey_ShouldReturnFailure_WhenExceptionThrownDuringDeserialization()
    {
        // Arrange
        CofferConfig config = new() { Url = "http://coffer-service" };
        _mockConfig.Value.Returns(config);

        HttpMessageHandlerMock mockHttpMessageHandler = new(HttpStatusCode.OK, "invalid json");
        HttpClient client = new(mockHttpMessageHandler);
        _mockHttpClientFactory.CreateClient().Returns(client);

        // Act
        Result<string> result = await _cofferService.GetKey("app", "secretName");

        // Assert
        Assert.False(result.Success);
        Assert.Single(result.Errors!);
        Assert.Equal(HttpStatusCode.InternalServerError, result.Errors?.First().Code);
        Assert.Equal("Cannot decode response", result.Errors?.First().Message);
    }

    [Fact]
    public async Task GetKey_ShouldReturnSuccess_WhenResponseIsValid()
    {
        // Arrange
        var config = new CofferConfig { Url = "http://coffer-service" };
        _mockConfig.Value.Returns(config);

        var responseContent = JsonConvert.SerializeObject(new CofferResponse { Data = "expected-key" });

        var mockHttpMessageHandler = new HttpMessageHandlerMock(HttpStatusCode.OK, responseContent);
        var client = new HttpClient(mockHttpMessageHandler);
        _mockHttpClientFactory.CreateClient().Returns(client);

        // Act
        var result = await _cofferService.GetKey("app", "secretName");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("expected-key", result.Data);
    }

    private class HttpMessageHandlerMock(HttpStatusCode statusCode, string responseContent = "") : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            HttpResponseMessage response = new(statusCode)
            {
                Content = new StringContent(responseContent)
            };
            return Task.FromResult(response);
        }
    }
}