namespace Application.Game.Card.Session;

public record GetSessionCommand(long CardId, string Mac) : ICommandWrapper<string>;

public class GetSessionCommandHandler : CommandHandlerBase<GetSessionCommand, string>
{
    private const string SESSION_XPATH = "/root/session";
    
    public GetSessionCommandHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override ValueTask<ServiceResult<string>> Handle(GetSessionCommand request, CancellationToken cancellationToken)
    {
        var session = new SessionDto
        {
            CardId = request.CardId,
            Mac = request.Mac,
            PlayerId = 1,
            Expires = 9999,
            SessionId = "12345678901234567890123456789012"
        };
        return ValueTask.FromResult(new ServiceResult<string>(session.SerializeCardData(SESSION_XPATH)));
    }
}