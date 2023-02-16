namespace Application.Game.Card.OnlineMatching;

public record StartOnlineMatchingCommand(long CardId, string Data) : IRequestWrapper<string>;

public class StartOnlineMatchingCommandHandler : RequestHandlerBase<StartOnlineMatchingCommand, string>
{
    public StartOnlineMatchingCommandHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override Task<ServiceResult<string>> Handle(StartOnlineMatchingCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}