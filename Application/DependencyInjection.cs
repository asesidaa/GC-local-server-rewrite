using System.Reflection;
using Application.Common.Behaviours;
using Application.Game.Card;
using Application.Interfaces;
using Application.Jobs;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(Assembly.GetExecutingAssembly());

        services.AddScoped<ICardDependencyAggregate, CardDependencyAggregate>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
        services.AddQuartz(q =>
        {
            q.UseMicrosoftDependencyInjectionJobFactory();
            var jobKey = new JobKey("UpdatePlayNumRankJob");
            q.AddJob<UpdatePlayNumRankJob>(options => options.WithIdentity(jobKey));

            q.AddTrigger(options =>
            {
                options.ForJob(jobKey)
                    .WithIdentity("UpdatePlayNumRankJob-trigger")
                    .StartNow()
                    .WithSimpleSchedule(x =>
                    {
                        x.WithIntervalInHours(24).RepeatForever();
                    });
            });
        });
        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });
        return services;
    }
}