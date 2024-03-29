﻿using System.Reflection;
using Application.Common.Behaviours;
using Application.Game.Card;
using Application.Jobs;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, int refreshIntervalHours = 24)
    {
        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

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
                        x.WithIntervalInHours(refreshIntervalHours).RepeatForever();
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
                        x.WithIntervalInHours(refreshIntervalHours).RepeatForever();
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
                        x.WithIntervalInHours(refreshIntervalHours).RepeatForever();
                    });
            });
            
            q.AddJob<MaintainNullValuesJob>(options => options.WithIdentity(MaintainNullValuesJob.KEY));
            q.AddTrigger(options =>
            {
                options.ForJob(MaintainNullValuesJob.KEY)
                    .WithIdentity("MaintainNullValuesJob-trigger")
                    .StartNow()
                    .WithSimpleSchedule(x =>
                    {
                        x.WithIntervalInHours(refreshIntervalHours).RepeatForever();
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