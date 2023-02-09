using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace Application.Common.Behaviours;

public class LoggingBehaviour<TRequest> : IRequestPreProcessor<TRequest> where TRequest : notnull
{
    private readonly ILogger<TRequest> logger;

    // ReSharper disable once ContextualLoggerProblem
    public LoggingBehaviour(ILogger<TRequest> logger)
    {
        this.logger = logger;
    }

    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        logger.LogInformation("Received request: {RequestName}, content: {Request}", requestName, request);

        return Task.CompletedTask;
    }
}