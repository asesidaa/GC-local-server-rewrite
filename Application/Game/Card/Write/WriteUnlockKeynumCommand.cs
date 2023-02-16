namespace Application.Game.Card.Write;

public record WriteUnlockKeynumCommand(long CardId, string Data) : IRequestWrapper<string>;

public class WriteUnlockKeynumCommandHandler : RequestHandlerBase<WriteUnlockKeynumCommand, string>
{
    public WriteUnlockKeynumCommandHandler(ICardDependencyAggregate aggregate) : base(aggregate) {}

    public override Task<ServiceResult<string>> Handle(WriteUnlockKeynumCommand request, CancellationToken cancellationToken)
    {
        // TODO: Add proper implementation
        return Task.FromResult(new ServiceResult<string>(request.Data));
    }
}