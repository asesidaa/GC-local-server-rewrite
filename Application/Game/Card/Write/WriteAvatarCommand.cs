namespace Application.Game.Card.Write;

public record WriteAvatarCommand(long CardId, string Data) : ICommandWrapper<string>;

public class WriteAvatarCommandHandler : CommandHandlerBase<WriteAvatarCommand, string>
{
    public WriteAvatarCommandHandler(ICardDependencyAggregate aggregate) : base(aggregate) {}

    public override ValueTask<ServiceResult<string>> Handle(WriteAvatarCommand request, CancellationToken cancellationToken)
    {
        // TODO: Add proper implementation
        return ValueTask.FromResult(new ServiceResult<string>(request.Data));
    }
}