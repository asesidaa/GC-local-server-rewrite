using System.ComponentModel.DataAnnotations;
using Application.Interfaces;
using Domain.Config;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MultiplayerTests.MainServer;

public class OnlineMatchingRegistrationTests
{
    [Fact]
    public void InfrastructureRegistration_ResolvesSingletonMatchingDependencies()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IOptions<OnlineMatchingConfig>>(Options.Create(new OnlineMatchingConfig()));
        services.AddInfrastructure(new ConfigurationBuilder().Build());
        using var provider = services.BuildServiceProvider();

        var firstClock = provider.GetRequiredService<TimeProvider>();
        var secondClock = provider.GetRequiredService<TimeProvider>();
        var firstGenerator = provider.GetRequiredService<IMatchingIdGenerator>();
        var secondGenerator = provider.GetRequiredService<IMatchingIdGenerator>();
        var firstService = provider.GetRequiredService<IOnlineMatchingService>();
        var secondService = provider.GetRequiredService<IOnlineMatchingService>();

        Assert.Same(firstClock, secondClock);
        Assert.Same(firstGenerator, secondGenerator);
        Assert.Same(firstService, secondService);
    }

    [Fact]
    public void CheckedInMatchingConfig_BindsApprovedDefaults()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "MainServer",
            "Configurations",
            "matching.json"));
        var builder = new ConfigurationBuilder();
        using (var stream = File.OpenRead(path))
        {
            builder.AddJsonStream(stream);
            var configuration = builder.Build();
            var section = configuration.GetSection(OnlineMatchingConfig.SECTION);
            Assert.True(section.GetChildren().Any(), "The checked-in OnlineMatching section must exist.");

            var services = new ServiceCollection();
            services.AddOptions<OnlineMatchingConfig>()
                .Bind(section)
                .ValidateDataAnnotations();
            using var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<IOptions<OnlineMatchingConfig>>().Value;

            Assert.Equal(99, options.SearchTimeoutSeconds);
            Assert.Equal(40, options.FormationTimeoutSeconds);
            Assert.Equal(10, options.PollIntervalSeconds);
            Assert.Equal(600, options.RoomIdleExpirySeconds);
            Assert.Equal(120, options.TerminalRetentionSeconds);
            Assert.Equal(600, options.MatchingIdReuseGuardSeconds);
            Assert.Equal(4, options.MaxPlayers);
        }
    }

    [Fact]
    public void ConfigurationValidation_RejectsMoreThanFourPlayers()
    {
        var options = new OnlineMatchingConfig { MaxPlayers = 5 };
        var results = new List<ValidationResult>();

        var valid = Validator.TryValidateObject(options, new ValidationContext(options), results, true);

        Assert.False(valid);
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(OnlineMatchingConfig.MaxPlayers)));
    }
}
