namespace Application.Game.Card.Write;

public record WriteSkinCommand(long CardId, string Data) : ICommandWrapper<string>;

public class WriteSkinCommandHandler : CommandHandlerBase<WriteSkinCommand, string>
{
    public WriteSkinCommandHandler(ICardDependencyAggregate aggregate) : base(aggregate) {}

    public override ValueTask<ServiceResult<string>> Handle(WriteSkinCommand request, CancellationToken cancellationToken)
    {
        // TODO: Add proper implementation
        return ValueTask.FromResult(new ServiceResult<string>(request.Data));
    }
}