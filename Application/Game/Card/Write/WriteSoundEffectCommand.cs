namespace Application.Game.Card.Write;

public record WriteSoundEffectCommand(long CardId, string Data) : IRequestWrapper<string>;

public class WriteSoundEffectCommandHandler : RequestHandlerBase<WriteSoundEffectCommand, string>
{
    public WriteSoundEffectCommandHandler(ICardDependencyAggregate aggregate) : base(aggregate) {}

    public override Task<ServiceResult<string>> Handle(WriteSoundEffectCommand request, CancellationToken cancellationToken)
    {
        // TODO: Add proper implementation
        return Task.FromResult(new ServiceResult<string>(request.Data));
    }
}