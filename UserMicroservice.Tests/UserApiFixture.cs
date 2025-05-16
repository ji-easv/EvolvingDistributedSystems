namespace UserMicroservice.Tests;

public class UserApiFixture : IDisposable
{
    private readonly IHost _server;
    public Uri ServerUri { get; }

    public UserApiFixture()
    {
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