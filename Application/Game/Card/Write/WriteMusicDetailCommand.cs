namespace Application.Game.Card.Write;

public record WriteMusicDetailCommand(long CardId, string Data) : IRequestWrapper<string>;

public class WriteMusicDetailCommandHandler : CardRequestHandlerBase<WriteMusicDetailCommand, string>
{
    public WriteMusicDetailCommandHandler(ICardDependencyAggregate aggregate) : base(aggregate) {}

    public override Task<ServiceResult<string>> Handle(WriteMusicDetailCommand request, CancellationToken cancellationToken)
    {
        // TODO: Add proper implementation
        return Task.FromResult(new ServiceResult<string>(request.Data));
    }
}