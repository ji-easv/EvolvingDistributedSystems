using UserMicroservice.Application;
using UserMicroservice.Infrastructure;

namespace UserMicroservice.Tests;

public class TestStartup(IConfiguration configuration)
{
    private readonly Startup _inner = new(configuration);

    public void ConfigureServices(IServiceCollection services)
    {
        _inner.ConfigureServices(services);
        
        services.AddSingleton<IUserRepository, MockUserRepository>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        _inner.Configure(app, env);
        
        app.UseMiddleware<ProviderStateMiddleware>();
    }
}