using UserMicroservice.Infrastructure;

namespace UserMicroservice.Tests;

public class TestStartup(IConfiguration configuration)
{
    private readonly Startup _inner = new(configuration);

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IUserRepository, MockUserRepository>();

        _inner.ConfigureServices(services);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseMiddleware<ProviderStateMiddleware>();

        _inner.Configure(app, env);
    }
}