using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Common.Behaviours;

public class UnhandledExceptionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<TRequest> logger;

    // ReSharper disable once ContextualLoggerProblem
    public UnhandledExceptionBehaviour(ILogger<TRequest> logger)
    {
        this.logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,  CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            var requestName = typeof(TRequest).Name;

            logger.LogError(ex, "Unhandled Exception for Request {Name} {@Request}", requestName, request);

            throw;
        }
    }

}