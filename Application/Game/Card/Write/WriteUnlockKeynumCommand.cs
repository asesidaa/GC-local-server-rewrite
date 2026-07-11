namespace Application.Game.Card.Write;

public record WriteUnlockKeynumCommand(long CardId, string Data) : ICommandWrapper<string>;

public class WriteUnlockKeynumCommandHandler : CommandHandlerBase<WriteUnlockKeynumCommand, string>
{
    public WriteUnlockKeynumCommandHandler(ICardDependencyAggregate aggregate) : base(aggregate) {}

    public override ValueTask<ServiceResult<string>> Handle(WriteUnlockKeynumCommand request, CancellationToken cancellationToken)
    {
        // TODO: Add proper implementation
        return ValueTask.FromResult(new ServiceResult<string>(request.Data));
    }
}