namespace Application.Game.Card.Management;

public record CardReissueCommand(long CardId) : ICommandWrapper<string>;

public class ReissueCommandHandler : CommandHandlerBase<CardReissueCommand, string>
{
    public ReissueCommandHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override ValueTask<ServiceResult<string>> Handle(CardReissueCommand request, CancellationToken cancellationToken)
    {
        // TODO: Support actual reissue
        return ValueTask.FromResult(ServiceResult.Failed<string>(ServiceError.NotReissue));
    }
}