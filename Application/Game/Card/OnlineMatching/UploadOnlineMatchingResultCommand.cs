namespace Application.Game.Card.OnlineMatching;

public record UploadOnlineMatchingResultCommand(long CardId, string Data) : IRequestWrapper<string>;

public class UploadOnlineMatchingResultCommandHandler : CardRequestHandlerBase<UploadOnlineMatchingResultCommand, string>
{
    public UploadOnlineMatchingResultCommandHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override Task<ServiceResult<string>> Handle(UploadOnlineMatchingResultCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}