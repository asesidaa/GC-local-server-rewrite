namespace Application.Game.Card.Write;

public record WriteAvatarCommand(long CardId, string Data) : IRequestWrapper<string>;

public class WriteAvatarCommandHandler : RequestHandlerBase<WriteAvatarCommand, string>
{
    public WriteAvatarCommandHandler(ICardDependencyAggregate aggregate) : base(aggregate) {}

    public override Task<ServiceResult<string>> Handle(WriteAvatarCommand request, CancellationToken cancellationToken)
    {
        // TODO: Add proper implementation
        return Task.FromResult(new ServiceResult<string>(request.Data));
    }
}