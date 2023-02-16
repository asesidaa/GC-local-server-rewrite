namespace Application.Game.Card.Write;

public record WriteSkinCommand(long CardId, string Data) : IRequestWrapper<string>;

public class WriteSkinCommandHandler : RequestHandlerBase<WriteSkinCommand, string>
{
    public WriteSkinCommandHandler(ICardDependencyAggregate aggregate) : base(aggregate) {}

    public override Task<ServiceResult<string>> Handle(WriteSkinCommand request, CancellationToken cancellationToken)
    {
        // TODO: Add proper implementation
        return Task.FromResult(new ServiceResult<string>(request.Data));
    }
}