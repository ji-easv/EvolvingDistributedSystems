using UserMicroservice.Application;
using UserMicroservice.Infrastructure;

namespace UserMicroservice.Tests;

public class TestStartup(IConfiguration configuration)
{
    private readonly Startup _inner = new(configuration);

    public void ConfigureServices(IServiceCollection services)
    {
        _inner.ConfigureServices(services);
        
        services.AddScoped<IUserService, UserService>();
        services.AddSingleton<IUserRepository, MockUserRepository>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseMiddleware<ProviderStateMiddleware>();
        
        _inner.Configure(app, env);
    }
}