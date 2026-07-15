namespace Application.Game.Card.Write;

public record WriteCoinCommand(long CardId, string Data) : ICommandWrapper<string>;

public class WriteCoinCommandHandler : CommandHandlerBase<WriteCoinCommand, string>
{
    public WriteCoinCommandHandler(ICardDependencyAggregate aggregate) : base(aggregate) {}

    public override ValueTask<ServiceResult<string>> Handle(WriteCoinCommand request, CancellationToken cancellationToken)
    {
        // TODO: Add proper implementation
        return ValueTask.FromResult(new ServiceResult<string>(request.Data));
    }
}