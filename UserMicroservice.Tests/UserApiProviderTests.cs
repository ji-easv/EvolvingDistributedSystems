using PactNet;
using PactNet.Infrastructure.Outputters;
using PactNet.Output.Xunit;
using PactNet.Verifier;
using UserMicroservice.Tests.Setup;
using Xunit.Abstractions;

namespace UserMicroservice.Tests;

public class UserApiProviderTests(
    UserApiFixture userApiFixture, 
    ITestOutputHelper output
    ) : IClassFixture<UserApiFixture>
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
        },
    });
    
    [Fact]
    public void EnsureUserMsHonoursPactWithGroupMsConsumer()
    {
        _pactVerifier
            .WithHttpEndpoint(userApiFixture.ServerUri)
            .WithPactBrokerSource(new Uri(userApiFixture.Options.PactBrokerUri), configure =>
            {
                configure.BasicAuthentication(userApiFixture.Options.PactBrokerUsername, userApiFixture.Options.PactBrokerPassword);
                
                if (Environment.GetEnvironmentVariable("PACT_PUBLISH_VERIFICATION_RESULTS") == "true") // Only publish results on CI/CD
                {
                    var version = Environment.GetEnvironmentVariable("GIT_SHA");
                    var branch = Environment.GetEnvironmentVariable("GIT_BRANCH");
                    var tags = Environment.GetEnvironmentVariable("GIT_TAGS").Split(",");
                    
                    configure.PublishResults(version, publish =>
                    {
                        publish.ProviderBranch(branch);
                        publish.ProviderTags(tags);
                    });
                }
            })
            .WithProviderStateUrl(new Uri(userApiFixture.ServerUri, "/provider-states"))
            .Verify();
    }
}