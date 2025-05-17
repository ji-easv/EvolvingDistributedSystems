using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using FluentAssertions;
using GroupMicroservice.Application;
using GroupMicroservice.Domain.DTOs;
using GroupMicroservice.Domain.Exceptions;
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
            PactDir =
                $"{Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName}{Path.DirectorySeparatorChar}pacts",
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
        var id = Guid.Parse("03247A5F-B7C9-4B88-8A6B-A3B583594EFD");
        
        // Arrange
        _pact.UponReceiving("A valid request for a single user by ID")
            .Given("a user with id {id} exists", new Dictionary<string, string> { ["id"] = id.ToString() })
            .WithRequest(HttpMethod.Get, $"/api/v1/user/{id}")
            .WithHeader("Accept", "application/json")
            .WillRespond()
            .WithStatus(HttpStatusCode.OK)
            .WithJsonBody(new
                {
                    id,
                    email = "string",
                    nickname = "string",
                    createdAt = "2009-07-27T00:00:00"
                }
            );

        await _pact.VerifyAsync(async ctx =>
        {
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
            user.Should().Match<GetUserDto>(u =>
                u.Id == id &&
                !string.IsNullOrEmpty(u.Email) &&
                !string.IsNullOrEmpty(u.Nickname) &&
                u.CreatedAt != default);
        });
    }

    [Fact]
    public async Task GetUserAsync_WhenCalled_ReturnsNotFound()
    {
        var id = Guid.Parse("DF721117-36AB-4B87-A9EC-2319A56F53C9");

        // Arrange
        _pact.UponReceiving("A request for a single user by ID that does not exist")
            .Given("a user with id {id} does not exist", new Dictionary<string, string> { ["id"] = id.ToString() })
            .WithRequest(HttpMethod.Get, $"/api/v1/user/{id}")
            .WithHeader("Accept", "application/json")
            .WillRespond()
            .WithStatus(HttpStatusCode.NotFound);

        await _pact.VerifyAsync(async ctx =>
        {
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
    
    [Fact]
    public async Task GetUsersByIdsAsync_WhenCalled_ReturnsListOfGetUserDto()
    {
        var userIds = new List<Guid>
        {
            Guid.Parse("03247A5F-B7C9-4B88-8A6B-A3B583594EFD"),
            Guid.Parse("A1B2C3D4-E5F6-7890-1234-56789ABCDEF0")
        };

        var expectedUsers = new List<object>
        {
            new
            {
                id = userIds[0],
                email = "user1@example.com",
                nickname = "User1",
                createdAt = "2023-01-01T00:00:00"
            },
            new
            {
                id = userIds[1],
                email = "user2@example.com",
                nickname = "User2",
                createdAt = "2023-01-02T00:00:00"
            }
        };

        // Arrange
        _pact.UponReceiving("A valid batch request for users by IDs")
            .WithRequest(HttpMethod.Post, "/api/v1/user/batch")
            .WithHeader("Content-Type", "application/json")
            .WithJsonBody(userIds)
            .WillRespond()
            .WithStatus(HttpStatusCode.OK)
            .WithJsonBody(expectedUsers);

        await _pact.VerifyAsync(async ctx =>
        {
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
            var users = await client.GetUsersByIdsAsync(userIds);

            // Assert structure
            users.Should().NotBeNullOrEmpty();
            users.Should().AllBeOfType<GetUserDto>();
            users.Should().HaveCount(2);
        });
    }
    
    [Fact]
    public async Task GetUsersByIdsAsync_WhenCalled_ReturnsNotFound()
    {
        var userIds = new List<Guid>
        {
            Guid.Parse("DF721117-36AB-4B87-A9EC-2319A56F53C9"),
            Guid.Parse("A1B2C3D4-E5F6-7890-1234-56789ABCDEF0")
        };

        // Arrange
        _pact.UponReceiving("A request for users by IDs that do not exist")
            .WithRequest(HttpMethod.Post, "/api/v1/user/batch")
            .WithHeader("Content-Type", "application/json")
            .WithJsonBody(userIds)
            .WillRespond()
            .WithStatus(HttpStatusCode.NotFound);

        await _pact.VerifyAsync(async ctx =>
        {
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
            Func<Task> action = () => client.GetUsersByIdsAsync(userIds);
            var response = await action.Should().ThrowAsync<HttpRequestException>();
            response.And.Message.Should().Be("Users not found");
            response.And.StatusCode.Should().Be(HttpStatusCode.NotFound);
        });
    }
}