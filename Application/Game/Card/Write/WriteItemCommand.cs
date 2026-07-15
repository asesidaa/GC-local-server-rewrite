namespace Application.Game.Card.Write;

public record WriteItemCommand(long CardId, string Data) : ICommandWrapper<string>;

public class WriteItemCommandHandler : CommandHandlerBase<WriteItemCommand, string>
{
    public WriteItemCommandHandler(ICardDependencyAggregate aggregate) : base(aggregate) {}

    public override ValueTask<ServiceResult<string>> Handle(WriteItemCommand request, CancellationToken cancellationToken)
    {
        // TODO: Add proper implementation
        return ValueTask.FromResult(new ServiceResult<string>(request.Data));
    }
}