using System.Text;
using System.Text.Json;
using PactNet;
using UserMicroservice.Application;
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
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Initialises a new instance of the <see cref="ProviderStateMiddleware"/> class.
    /// </summary>
    /// <param name="next">Next request delegate</param>
    public ProviderStateMiddleware(RequestDelegate next, IUserRepository userRepository)
    {
        _next = next;
        _userRepository = userRepository;

        _providerStates = new Dictionary<string, Func<IDictionary<string, object>, HttpContext, Task>>
        {
            ["a user with id {id} exists"] = EnsureEventExistsAsync,
            ["a user with id {id} does not exist"] = async (parameters, httpContext) =>
            {
                var id = parameters["id"].ToString();
                var user = await userRepository.GetUserByIdAsync(Guid.Parse(id));
                if (user == null)
                {
                    return;
                }

                await userRepository.DeleteUserAsync(user);
            }
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
        var id =  Guid.Parse(parameters["id"].ToString());
        var existingUser = await _userRepository.GetUserByIdAsync(id);
        if (existingUser == null)
        {
            await _userRepository.CreateUserAsync(new UserEntity
            {
                Id = id,
                Email = "test@example.com",
                Nickname = "Bobby",
                CreatedAt = new DateTime(2009, 7, 27, 0, 0, 0),
                Password = "password"
            });
        }
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
                    var paramDictionary = providerState.Params.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
                    await _providerStates[providerState.State].Invoke(
                        paramDictionary,
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