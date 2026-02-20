using System.Net;
using System.Text.Json;
using Azure.API.Config;
using Azure.API.Models.WorkItems;
using Azure.API.Models.WorkItems.Requests;
using Azure.API.Models.WorkItems.Responses;
using Azure.API.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace Azure.API.Tests.UnitTests.WorkItems;

public class WorkItemServiceTests
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    private readonly Mock<IHttpClientFactory> httpClientFactoryMock;
    private readonly Mock<HttpMessageHandler> httpMessageHandlerMock;
    private readonly Mock<IOptions<AzureDevOpsOptions>> optionsMock;
    private readonly Mock<ILogger<WorkItemService>> loggerMock;
    private readonly WorkItemService service;

    public WorkItemServiceTests()
    {
        this.httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        this.httpClientFactoryMock = new Mock<IHttpClientFactory>();

        var client = new HttpClient(this.httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri($"{this.optionsMock?.Object.Value.BaseUrl}")
        };

        this.httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(client);

        this.optionsMock = new Mock<IOptions<AzureDevOpsOptions>>();
        this.optionsMock.Setup(x => x.Value).Returns(new AzureDevOpsOptions
        {
            Organization = "test-org",
            Project = "test-project",
            DefaultType = "Task",
            SupportedTypes = new List<string> { "Task", "Bug" },
            Pat = string.Empty,
            BaseUrl = "https://dev.azure.com/test-org/test-project/_apis/wit",
            EntitlementsBaseUrl = "https://vsaex.dev.azure.com/test-org"
        });

        this.loggerMock = new Mock<ILogger<WorkItemService>>();

        this.service = new WorkItemService(this.optionsMock.Object, this.httpClientFactoryMock.Object, this.loggerMock.Object);
    }

    [Fact]
    public async Task GetWorkItemListAsync_ShouldReturnItems_WhenApiCallIsSuccessful()
    {
        // Arrange
        var ids = new[] { 1, 2 };
        var response = new WorkItemListResponse
        {
            Count = 2,
            Value = new List<WorkItemResponse>
            {
                new() { Id = 1, Fields = new Dictionary<string, object> { { "System.Title", "Task 1" } } },
                new() { Id = 2, Fields = new Dictionary<string, object> { { "System.Title", "Task 2" } } }
            }
        };

        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response, JsonOptions));

        // Act
        var result = await this.service.GetWorkItemListAsync(ids);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(2);
        result.Value.Should().HaveCount(2);
        result.Value[0].Id.Should().Be(1);
    }

    [Fact]
    public async Task QueryWorkItemIdsAsync_ShouldReturnIds_WhenApiCallIsSuccessful()
    {
        // Arrange
        var response = new WiqlResponse
        {
            WorkItems = new List<WorkItemReference>
            {
                new() { Id = 10 },
                new() { Id = 20 }
            }
        };

        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await this.service.QueryWorkItemIdsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().ContainInOrder(10, 20);
    }

    [Fact]
    public async Task CreateWorkItemAsync_ShouldReturnId_WhenApiCallIsSuccessful()
    {
        // Arrange
        var request = new WorkItemRequest { Title = "New Task" };
        var createdId = "123";

        SetupHttpResponse(HttpStatusCode.OK, createdId);

        // Act
        var result = await this.service.CreateWorkItemAsync(request);

        // Assert
        result.Should().Be(createdId);
    }

    [Fact]
    public async Task UpdateWorkItemAsync_ShouldReturnUpdatedItem_WhenApiCallIsSuccessful()
    {
        // Arrange
        var request = new WorkItemRequest { Title = "Updated Task" };
        var updatedResponse = "{\"id\": 1, \"fields\": {\"System.Title\": \"Updated Task\"}}";
        SetupHttpResponse(HttpStatusCode.OK, updatedResponse);

        // Act
        var result = await this.service.UpdateWorkItemAsync(1, request);

        // Assert
        result.Should().Contain("Updated Task");
    }

    [Fact]
    public async Task DeleteWorkItemAsync_ShouldReturnTrue_WhenApiCallIsSuccessful()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, "");

        // Act
        var result = await this.service.DeleteWorkItemAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteWorkItemAsync_ShouldThrow_WhenApiCallFails()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.NotFound, "Not Found");

        // Act
        Func<Task> act = async () => await this.service.DeleteWorkItemAsync(1);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, string content)
    {
        this.httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content)
            });
    }
}
