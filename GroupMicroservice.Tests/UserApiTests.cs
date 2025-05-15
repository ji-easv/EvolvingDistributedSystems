using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using FluentAssertions;
using GroupMicroservice.Application;
using GroupMicroservice.Domain.DTOs;
using Moq;
using PactNet;
using PactNet.Output.Xunit;
using Xunit.Abstractions;

namespace GroupMicroservice.Tests;

public class UserApiTests
{
    private readonly IPactBuilderV4 _pact;
    private readonly Mock<IHttpClientFactory> _mockFactory;

    public UserApiTests(ITestOutputHelper output)
    {
        _mockFactory = new Mock<IHttpClientFactory>();

        var config = new PactConfig
        {
            PactDir = $"{Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName}{Path.DirectorySeparatorChar}pacts",
            Outputters =
            [
                new XunitOutput(output)
            ],
            DefaultJsonSettings = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            },
            LogLevel = PactLogLevel.Debug
        };
        _pact = Pact.V4("GroupMicroservice", "UserMicroservice", config).WithHttpInteractions();
    }

    [Fact]
    public async Task GetUserAsync_WhenCalled_ReturnsGetUserDto()
    {
        // var id = Guid.Parse("03247A5F-B7C9-4B88-8A6B-A3B583594EFD");
        var id = Guid.NewGuid();

        var expectedUser = new GetUserDto
        {
            Id = id,
            Email = "test@example.com",
            Nickname = "Bobby",
            CreatedAt = new DateTime(2009, 7, 27, 0, 0, 0),
        };
        
        // Arrange
        _pact.UponReceiving("A valid request for a single user by ID")
                .Given($"a user with id {id} exists", new Dictionary<string, string> { ["id"] = id.ToString() })
                .WithRequest(HttpMethod.Get, $"/api/v1/user/{id}")
            .WillRespond()
                .WithStatus(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithJsonBody(expectedUser);

        await _pact.VerifyAsync(async ctx => {
           _mockFactory
               .Setup(f => f.CreateClient("UserApi"))
               .Returns(() => new HttpClient
               {
                   BaseAddress = ctx.MockServerUri,
                   DefaultRequestHeaders =
                   {
                       Accept = { MediaTypeWithQualityHeaderValue.Parse("application/json") }
                   }
               });
           
            var client = new UserApiClient(_mockFactory.Object);
            var user = await client.GetUserAsync(id);
            user.Should().BeEquivalentTo(expectedUser);
        });
    }
    
    [Fact]
    public async Task GetUserAsync_WhenCalled_ReturnsNotFound()
    {
        var id = Guid.Parse("DF721117-36AB-4B87-A9EC-2319A56F53C9");

        // Arrange
        _pact.UponReceiving("A valid request for a single user by ID")
                .Given($"a user with id {id} does not exist", new Dictionary<string, string> { ["id"] = id.ToString() })
                .WithRequest(HttpMethod.Get, $"/api/v1/user/{id}")
            .WillRespond()
                .WithStatus(HttpStatusCode.NotFound)
                .WithHeader("Content-Type", "application/json; charset=utf-8");

        await _pact.VerifyAsync(async ctx => {
            _mockFactory
                .Setup(f => f.CreateClient("UserApi"))
                .Returns(() => new HttpClient
                {
                    BaseAddress = ctx.MockServerUri,
                    DefaultRequestHeaders =
                    {
                        Accept = { MediaTypeWithQualityHeaderValue.Parse("application/json") }
                    }
                });

            var client = new UserApiClient(_mockFactory.Object);
            Func<Task> action = () => client.GetUserAsync(id);
            var response = await action.Should().ThrowAsync<HttpRequestException>();
            response.And.StatusCode.Should().Be(HttpStatusCode.NotFound);
        });
    }
}