using PactNet;
using PactNet.Infrastructure.Outputters;
using PactNet.Output.Xunit;
using PactNet.Verifier;
using Xunit.Abstractions;

namespace UserMicroservice.Tests;

public class UserApiProviderTests(UserApiFixture userApiFixture, ITestOutputHelper output) : IClassFixture<UserApiFixture>
{
    private readonly PactVerifier _pactVerifier = new("UserMicroservice", new PactVerifierConfig
    {
        LogLevel = PactLogLevel.Debug,
        Outputters = new List<IOutput>
        {
            // NOTE: PactNet defaults to a ConsoleOutput, however
            // xUnit 2 does not capture the console output, so this
            // sample creates a custom xUnit outputter. You will
            // have to do the same in xUnit projects.
            new XunitOutput(output)
        }
    });
    
    [Fact]
    public void EnsureUserMsHonoursPactWithGroupMsConsumer()
    {
        // Arrange
        var pactDir =
            $"{Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName}{Path.DirectorySeparatorChar}pacts";
        var pactFile = Path.Combine(pactDir, "GroupMicroservice-UserMicroservice.json");

        // Act / Assert
        _pactVerifier
            .WithHttpEndpoint(userApiFixture.ServerUri)
            .WithFileSource(new FileInfo(pactFile))
            .WithProviderStateUrl(new Uri(userApiFixture.ServerUri, "/provider-states"))
            .Verify();
    }
}