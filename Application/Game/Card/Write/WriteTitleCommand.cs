namespace Application.Game.Card.Write;

public record WriteTitleCommand(long CardId, string Data) : ICommandWrapper<string>;

public class WriteTitleCommandHandler : CommandHandlerBase<WriteTitleCommand, string>
{
    public WriteTitleCommandHandler(ICardDependencyAggregate aggregate) : base(aggregate) {}

    public override ValueTask<ServiceResult<string>> Handle(WriteTitleCommand request, CancellationToken cancellationToken)
    {
        // TODO: Add proper implementation
        return ValueTask.FromResult(new ServiceResult<string>(request.Data));
    }
}