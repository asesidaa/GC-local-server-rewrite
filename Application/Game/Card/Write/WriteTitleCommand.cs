namespace Application.Game.Card.Write;

public record WriteTitleCommand(long CardId, string Data) : IRequestWrapper<string>;

public class WriteTitleCommandHandler : CardRequestHandlerBase<WriteTitleCommand, string>
{
    public WriteTitleCommandHandler(ICardDependencyAggregate aggregate) : base(aggregate) {}

    public override Task<ServiceResult<string>> Handle(WriteTitleCommand request, CancellationToken cancellationToken)
    {
        // TODO: Add proper implementation
        return Task.FromResult(new ServiceResult<string>(request.Data));
    }
}