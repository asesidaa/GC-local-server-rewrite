namespace Application.Game.Card.Write;

public record WriteMusicDetailCommand(long CardId, string Data) : ICommandWrapper<string>;

public class WriteMusicDetailCommandHandler : CommandHandlerBase<WriteMusicDetailCommand, string>
{
    public WriteMusicDetailCommandHandler(ICardDependencyAggregate aggregate) : base(aggregate) {}

    public override ValueTask<ServiceResult<string>> Handle(WriteMusicDetailCommand request, CancellationToken cancellationToken)
    {
        // TODO: Add proper implementation
        return ValueTask.FromResult(new ServiceResult<string>(request.Data));
    }
}