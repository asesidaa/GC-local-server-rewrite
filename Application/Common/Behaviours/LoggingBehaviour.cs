using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Common.Behaviours;

public sealed class LoggingBehaviour<TMessage, TResponse> : MessagePreProcessor<TMessage, TResponse>
    where TMessage : notnull, IMessage
{
    private readonly ILogger<TMessage> logger;

    public LoggingBehaviour(ILogger<TMessage> logger)
    {
        this.logger = logger;
    }

    protected override ValueTask Handle(TMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Received request: {RequestName}, content: {Request}",
            typeof(TMessage).Name,
            message);

        return ValueTask.CompletedTask;
    }
}
