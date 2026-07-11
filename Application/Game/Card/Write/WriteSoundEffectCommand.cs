namespace Application.Game.Card.Write;

public record WriteSoundEffectCommand(long CardId, string Data) : ICommandWrapper<string>;

public class WriteSoundEffectCommandHandler : CommandHandlerBase<WriteSoundEffectCommand, string>
{
    public WriteSoundEffectCommandHandler(ICardDependencyAggregate aggregate) : base(aggregate) {}

    public override ValueTask<ServiceResult<string>> Handle(WriteSoundEffectCommand request, CancellationToken cancellationToken)
    {
        // TODO: Add proper implementation
        return ValueTask.FromResult(new ServiceResult<string>(request.Data));
    }
}