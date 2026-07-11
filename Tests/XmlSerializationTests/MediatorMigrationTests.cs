using Application;
using Application.Api.Auth;
using Application.Common.Behaviours;
using Application.Game.Server;
using Application.Interfaces;
using Domain.Config;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Shared.Models;

namespace XmlSerializationTests;

public class MediatorMigrationTests
{
    private const string AdminPassword = "migration-test-password";

    [Fact]
    public void QueryWrapper_UsesNativeQueryContract()
    {
        Assert.Contains(
            typeof(IQuery<ServiceResult<int>>),
            typeof(IQueryWrapper<int>).GetInterfaces());
    }

    [Fact]
    public void CommandWrapper_UsesNativeCommandContract()
    {
        Assert.Contains(
            typeof(ICommand<ServiceResult<int>>),
            typeof(ICommandWrapper<int>).GetInterfaces());
    }

    [Fact]
    public async Task AddApplication_DispatchesNativeQuery()
    {
        var query = Assert.IsAssignableFrom<IQuery<ServiceResult<bool>>>(
            new ValidateAdminLoginQuery(AdminPassword));

        using var provider = CreateServiceProvider();
        using var scope = provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var result = await sender.Send(query);

        Assert.True(result.Succeeded);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task AddApplication_DispatchesNativeCommand()
    {
        var command = Assert.IsAssignableFrom<ICommand<string>>(
            new CertifyCommand(null, null, null, null, "localhost"));

        using var provider = CreateServiceProvider();
        using var scope = provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var result = await sender.Send(command);

        Assert.Equal("error=0", result);
    }

    [Fact]
    public void AddApplication_RegistersOrderedNativePipelineBehaviours()
    {
        using var provider = CreateServiceProvider();
        using var scope = provider.CreateScope();
        var serviceType = typeof(IPipelineBehavior<,>).MakeGenericType(
            typeof(ValidateAdminLoginQuery),
            typeof(ServiceResult<bool>));

        var behaviorTypes = scope.ServiceProvider
            .GetServices(serviceType)
            .Select(behavior => Assert.IsAssignableFrom<object>(behavior).GetType().GetGenericTypeDefinition())
            .ToArray();

        Assert.Equal(
            new[] { typeof(LoggingBehaviour<,>), typeof(UnhandledExceptionBehaviour<,>) },
            behaviorTypes);
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.Configure<WebAuthConfig>(options => options.AdminPassword = AdminPassword);
        services.Configure<AuthConfig>(_ => { });
        services.Configure<RelayConfig>(_ => { });
        services.AddApplication();
        return services.BuildServiceProvider();
    }
}
