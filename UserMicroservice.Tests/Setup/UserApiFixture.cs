using Microsoft.Extensions.Options;

namespace UserMicroservice.Tests.Setup;

public class UserApiFixture : IDisposable
{
    private readonly IHost _server;
    public Uri ServerUri { get; }
    public PactBrokerOptions Options { get; }

    public UserApiFixture()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var serviceProvider = new ServiceCollection()
            .Configure<PactBrokerOptions>(configuration.GetSection("PactBroker"))
            .BuildServiceProvider();

        Options = serviceProvider.GetRequiredService<IOptions<PactBrokerOptions>>().Value;
        
        ServerUri = new Uri("http://localhost:9223");
        _server = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseUrls(ServerUri.ToString());
                webBuilder.UseStartup<TestStartup>();
            })
            .Build();
        
        _server.Start();
    }

    public void Dispose()
    {
        _server.Dispose();
    }
}