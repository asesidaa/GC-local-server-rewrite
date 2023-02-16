namespace Application.Game.Card.OnlineMatching;

public record UpdateOnlineMatchingCommand(long CardId, string Data) : IRequestWrapper<string>;

public class UpdateOnlineMatchingCommandHandler : RequestHandlerBase<UpdateOnlineMatchingCommand, string>
{
    public UpdateOnlineMatchingCommandHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override Task<ServiceResult<string>> Handle(UpdateOnlineMatchingCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}