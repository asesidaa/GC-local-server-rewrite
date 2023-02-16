namespace Application.Game.Card.Management;

public record CardReissueCommand(long CardId) : IRequestWrapper<string>;

public class ReissueCommandHandler : RequestHandlerBase<CardReissueCommand, string>
{
    public ReissueCommandHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override Task<ServiceResult<string>> Handle(CardReissueCommand request, CancellationToken cancellationToken)
    {
        // TODO: Support actual reissue
        return Task.FromResult(ServiceResult.Failed<string>(ServiceError.NotReissue));
    }
}