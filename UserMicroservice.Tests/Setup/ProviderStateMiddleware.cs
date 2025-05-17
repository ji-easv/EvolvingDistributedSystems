using System.Text;
using System.Text.Json;
using PactNet;
using UserMicroservice.Domain.Entities;
using UserMicroservice.Infrastructure;

namespace UserMicroservice.Tests.Setup;

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
    /// <param name="userRepository"></param>
    public ProviderStateMiddleware(RequestDelegate next, IUserRepository userRepository)
    {
        _next = next;
        _userRepository = userRepository;

        _providerStates = new Dictionary<string, Func<IDictionary<string, object>, HttpContext, Task>>
        {
            ["a user with id {id} exists"] = EnsureUserExistsAsync,
            ["a user with id {id} does not exist"] = EnsureUserDoesNotExistAsync,
            ["users with the specified IDs exist"] = EnsureValidBatchRequestAsync,
            ["users with the specified IDs do not exist"] = EnsureInvalidBatchRequestAsync
        };
    }

    private async Task EnsureUserExistsAsync(IDictionary<string, object> parameters, HttpContext httpContext)
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
    
    private async Task EnsureUserDoesNotExistAsync(IDictionary<string, object> parameters, HttpContext httpContext)
    {
        var id = Guid.Parse(parameters["id"].ToString());
        var existingUser = await _userRepository.GetUserByIdAsync(id);
        if (existingUser != null)
        {
            await _userRepository.DeleteUserAsync(existingUser);
        }
    }
    
    private async Task EnsureValidBatchRequestAsync(IDictionary<string, object> arg1, HttpContext arg2)
    {
        var userIds = arg1["ids"].ToString().Split(",");
        foreach (var user in userIds)
        {
            var id = Guid.Parse(user);
            var existingUser = await _userRepository.GetUserByIdAsync(id);
            if (existingUser == null)
            {
                await _userRepository.CreateUserAsync(new UserEntity
                {
                    Id = id,
                    Email = $"{id.ToString()}@app.com",
                    Nickname = id.ToString(),
                    CreatedAt = new DateTime(2009, 7, 27, 0, 0, 0),
                    Password = "password"
                });
            }
        }
    }
    
    private async Task EnsureInvalidBatchRequestAsync(IDictionary<string, object> arg1, HttpContext arg2)
    {
        var userIds = arg1["ids"].ToString().Split(",");
        
        foreach (var user in userIds)
        {
            var id = Guid.Parse(user);
            var existingUser = await _userRepository.GetUserByIdAsync(id);
            if (existingUser != null)
            {
                await _userRepository.DeleteUserAsync(existingUser);
            }
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

        await HandleProviderStatesRequest(context);
    }
    
    private async Task HandleProviderStatesRequest(HttpContext context)
    {
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