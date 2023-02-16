namespace Application.Game.Card.Write;

public record WriteItemCommand(long CardId, string Data) : IRequestWrapper<string>;

public class WriteItemCommandHandler : RequestHandlerBase<WriteItemCommand, string>
{
    public WriteItemCommandHandler(ICardDependencyAggregate aggregate) : base(aggregate) {}

    public override Task<ServiceResult<string>> Handle(WriteItemCommand request, CancellationToken cancellationToken)
    {
        // TODO: Add proper implementation
        return Task.FromResult(new ServiceResult<string>(request.Data));
    }
}