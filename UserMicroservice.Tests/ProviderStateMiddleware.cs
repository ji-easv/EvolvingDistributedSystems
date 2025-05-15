using System.Text;
using System.Text.Json;
using PactNet;
using UserMicroservice.Domain.DTOs;
using UserMicroservice.Domain.Entities;
using UserMicroservice.Infrastructure;

namespace UserMicroservice.Tests;

/// <summary>
/// Middleware for handling provider state requests
/// </summary>
public class ProviderStateMiddleware
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IDictionary<string, Func<IDictionary<string, object>, HttpContext, Task>> _providerStates;
    private readonly RequestDelegate _next;
    private readonly IUserRepository _users;

    /// <summary>
    /// Initialises a new instance of the <see cref="ProviderStateMiddleware"/> class.
    /// </summary>
    /// <param name="next">Next request delegate</param>
    public ProviderStateMiddleware(RequestDelegate next)
    {
        _next = next;

        _providerStates = new Dictionary<string, Func<IDictionary<string, object>, HttpContext, Task>>
        {
            ["a user with id {id} exists"] = EnsureEventExistsAsync
        };
    }

    /// <summary>
    /// Ensure an event exists
    /// </summary>
    /// <param name="parameters">Event parameters</param>
    /// <param name="httpContext"></param>
    /// <returns>Awaitable</returns>
    private async Task EnsureEventExistsAsync(IDictionary<string, object> parameters, HttpContext httpContext)
    {
        var id = (JsonElement)parameters["id"];
        var userRepository = httpContext.RequestServices.GetRequiredService<IUserRepository>();
        
        await userRepository.CreateUserAsync(new UserEntity
        {
            Id = Guid.Parse(id.ToString()),
            Email = "test@example.com",
            Nickname = "Bobby",
            CreatedAt = new DateTime(2009, 7, 27, 0, 0, 0),
            Password = "password"
        });
    }

    /// <summary>
    /// Handle the request
    /// </summary>
    /// <param name="context">Request context</param>
    /// <returns>Awaitable</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        if (!(context.Request.Path.Value?.StartsWith("/provider-states") ?? false))
        {
            await _next.Invoke(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status200OK;

        if (context.Request.Method == HttpMethod.Post.ToString())
        {
            string jsonRequestBody;

            using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8))
            {
                jsonRequestBody = await reader.ReadToEndAsync();
            }

            try
            {
                var providerState = JsonSerializer.Deserialize<ProviderState>(jsonRequestBody, Options);

                if (!string.IsNullOrEmpty(providerState?.State))
                {
                    await _providerStates[providerState.State].Invoke(
                        providerState.Params.ToDictionary(kvp => kvp.Key, object (kvp) => kvp.Value),
                        context
                    );
                }
            }
            catch (Exception e)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("Failed to deserialise JSON provider state body:");
                await context.Response.WriteAsync(jsonRequestBody);
                await context.Response.WriteAsync(string.Empty);
                await context.Response.WriteAsync(e.ToString());
            }
        }
    }
}