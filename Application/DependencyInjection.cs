using System.Reflection;
using Application.Common.Behaviours;
using Application.Game.Card;
using Application.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(Assembly.GetExecutingAssembly());

        services.AddScoped<ICardDependencyAggregate, CardDependencyAggregate>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
        return services;
    }
}