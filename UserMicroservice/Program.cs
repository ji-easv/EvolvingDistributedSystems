namespace UserMicroservice;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = CreateHostBuilder(args);
        var host = builder.Build();
        host.Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
}