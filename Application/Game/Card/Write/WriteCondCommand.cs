namespace Application.Game.Card.Write;

public record WriteCondCommand(long CardId, string Data) : ICommandWrapper<string>;

public class WriteCondCommandHandler : CommandHandlerBase<WriteCondCommand, string>
{
    public WriteCondCommandHandler(ICardDependencyAggregate aggregate) : base(aggregate) {}

    public override ValueTask<ServiceResult<string>> Handle(WriteCondCommand request, CancellationToken cancellationToken)
    {
        // TODO: Add proper implementation
        return ValueTask.FromResult(new ServiceResult<string>(request.Data));
    }
}