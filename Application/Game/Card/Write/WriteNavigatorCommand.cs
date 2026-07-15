namespace Application.Game.Card.Write;

public record WriteNavigatorCommand(long CardId, string Data) : ICommandWrapper<string>;

public class WriteNavigatorCommandHandler : CommandHandlerBase<WriteNavigatorCommand, string>
{
    public WriteNavigatorCommandHandler(ICardDependencyAggregate aggregate) : base(aggregate) {}

    public override ValueTask<ServiceResult<string>> Handle(WriteNavigatorCommand request, CancellationToken cancellationToken)
    {
        // TODO: Add proper implementation
        return ValueTask.FromResult(new ServiceResult<string>(request.Data));
    }
}