using PactNet.Infrastructure.Outputters;
using PactNet.Output.Xunit;
using PactNet.Verifier;
using Xunit.Abstractions;

namespace UserMicroservice.Tests;

public class UserApiProviderTests(UserApiFixture userApiFixture, ITestOutputHelper output) : IClassFixture<UserApiFixture>
{
    [Fact]
    public void EnsureSomethingApiHonoursPactWithConsumer()
    {
        // Arrange
        var config = new PactVerifierConfig
        {
            Outputters = new List<IOutput>
            {
                // NOTE: PactNet defaults to a ConsoleOutput, however
                // xUnit 2 does not capture the console output, so this
                // sample creates a custom xUnit outputter. You will
                // have to do the same in xUnit projects.
                new XunitOutput(output),
            },
        };
        
        var pactDir =
            $"{Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName}{Path.DirectorySeparatorChar}pacts";
        var pactFile = Path.Combine(pactDir, "GroupMicroservice-UserMicroservice.json");

        // Act / Assert
        using var pactVerifier = new PactVerifier("UserMicroservice", config);

        pactVerifier
            .WithHttpEndpoint(userApiFixture.ServerUri)
            .WithFileSource(new FileInfo(pactFile))
            .WithProviderStateUrl(new Uri(userApiFixture.ServerUri, "/provider-states"))
            .Verify();
    }
}