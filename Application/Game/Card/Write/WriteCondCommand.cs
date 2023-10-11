namespace Application.Game.Card.Write;

public record WriteCondCommand(long CardId, string Data) : IRequestWrapper<string>;

public class WriteCondCommandHandler : RequestHandlerBase<WriteCondCommand, string>
{
    public WriteCondCommandHandler(ICardDependencyAggregate aggregate) : base(aggregate) {}

    public override Task<ServiceResult<string>> Handle(WriteCondCommand request, CancellationToken cancellationToken)
    {
        // TODO: Add proper implementation
        return Task.FromResult(new ServiceResult<string>(request.Data));
    }
}