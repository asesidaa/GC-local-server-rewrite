namespace Application.Game.Card.OnlineMatching;

public record StartOnlineMatchingCommand(long CardId, string Data) : ICommandWrapper<string>;

public sealed class StartOnlineMatchingCommandHandler : ICommandHandlerWrapper<StartOnlineMatchingCommand, string>
{
    private readonly IOnlineMatchingService matchingService;
    private readonly TimeProvider timeProvider;

    public StartOnlineMatchingCommandHandler(IOnlineMatchingService matchingService, TimeProvider timeProvider)
    {
        this.matchingService = matchingService;
        this.timeProvider = timeProvider;
    }

    public ValueTask<ServiceResult<string>> Handle(
        StartOnlineMatchingCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var dto = request.Data.DeserializeCardData<OnlineMatchingEntryRequestDto>();
            var participant = OnlineMatchingXml.ToParticipant(dto, request.CardId, timeProvider.GetLocalNow());
            var result = matchingService.Enter(participant);
            return ValueTask.FromResult(result.IsSuccess
                ? new ServiceResult<string>(OnlineMatchingXml.SerializeSnapshot(result.Value!))
                : ServiceResult.Failed<string>(ServiceError.CustomMessage(result.Message!)));
        }
        catch (OnlineMatchingProtocolException ex)
        {
            return ValueTask.FromResult(ServiceResult.Failed<string>(ServiceError.CustomMessage(ex.Message)));
        }
    }
}
