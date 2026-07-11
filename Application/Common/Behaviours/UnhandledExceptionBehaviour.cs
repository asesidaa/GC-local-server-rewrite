using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Common.Behaviours;

public sealed class UnhandledExceptionBehaviour<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : notnull, IMessage
{
    private readonly ILogger<TMessage> logger;

    public UnhandledExceptionBehaviour(ILogger<TMessage> logger)
    {
        this.logger = logger;
    }

    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next(message, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unhandled Exception for Request {Name} {@Request}",
                typeof(TMessage).Name,
                message);

            throw;
        }
    }
}
