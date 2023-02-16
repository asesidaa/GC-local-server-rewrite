namespace Application.Game.Card.Write;

public record WriteNavigatorCommand(long CardId, string Data) : IRequestWrapper<string>;

public class WriteNavigatorCommandHandler : RequestHandlerBase<WriteNavigatorCommand, string>
{
    public WriteNavigatorCommandHandler(ICardDependencyAggregate aggregate) : base(aggregate) {}

    public override Task<ServiceResult<string>> Handle(WriteNavigatorCommand request, CancellationToken cancellationToken)
    {
        // TODO: Add proper implementation
        return Task.FromResult(new ServiceResult<string>(request.Data));
    }
}