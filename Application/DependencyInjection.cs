using System.Reflection;
using Application.Common.Behaviours;
using Application.Game.Card;
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
            
            q.AddJob<UpdatePlayNumRankJob>(options => options.WithIdentity(UpdatePlayNumRankJob.KEY));
            q.AddTrigger(options =>
            {
                options.ForJob(UpdatePlayNumRankJob.KEY)
                    .WithIdentity("UpdatePlayNumRankJob-trigger")
                    .StartNow()
                    .WithSimpleSchedule(x =>
                    {
                        x.WithIntervalInHours(24).RepeatForever();
                    });
            });
            
            q.AddJob<UpdateGlobalScoreRankJob>(options => options.WithIdentity(UpdateGlobalScoreRankJob.KEY));
            q.AddTrigger(options =>
            {
                options.ForJob(UpdateGlobalScoreRankJob.KEY)
                    .WithIdentity("UpdateGlobalScoreRankJob-trigger")
                    .StartNow()
                    .WithSimpleSchedule(x =>
                    {
                        x.WithIntervalInHours(24).RepeatForever();
                    });
            });
            
            q.AddJob<UpdateMonthlyScoreRankJob>(options => options.WithIdentity(UpdateMonthlyScoreRankJob.KEY));
            q.AddTrigger(options =>
            {
                options.ForJob(UpdateMonthlyScoreRankJob.KEY)
                    .WithIdentity("UpdateMonthlyScoreRankJob-trigger")
                    .StartNow()
                    .WithSimpleSchedule(x =>
                    {
                        x.WithIntervalInHours(24).RepeatForever();
                    });
            });
            
            q.AddJob<MaintainTenpoIdJob>(options => options.WithIdentity(MaintainTenpoIdJob.KEY));
            q.AddTrigger(options =>
            {
                options.ForJob(MaintainTenpoIdJob.KEY)
                    .WithIdentity("MaintainTenpoIdJob-trigger")
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