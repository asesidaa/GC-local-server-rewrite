using Microsoft.Extensions.Logging;

namespace Application.Game.Card.OnlineMatching;

public record UpdateOnlineMatchingCommand(long CardId, string Data) : IRequestWrapper<string>;

public class UpdateOnlineMatchingCommandHandler : IRequestHandlerWrapper<UpdateOnlineMatchingCommand, string>
{
    private readonly IOnlineMatchingService matchingService;

    private readonly ILogger<UpdateOnlineMatchingCommandHandler> logger;

    public UpdateOnlineMatchingCommandHandler(IOnlineMatchingService matchingService,
        ILogger<UpdateOnlineMatchingCommandHandler> logger)
    {
        this.matchingService = matchingService;
        this.logger = logger;
    }

    public Task<ServiceResult<string>> Handle(UpdateOnlineMatchingCommand request, CancellationToken cancellationToken)
    {
        var data = request.Data.DeserializeCardData<OnlineMatchEntryDto>();

        var snapshot = matchingService.UpdateRoom(data.MatchId, request.CardId, data.MessageId);
        if (snapshot is null)
        {
            logger.LogWarning("Match id {MatchId} not found", data.MatchId);
            return Task.FromResult(
                ServiceResult.Failed<string>(ServiceError.CustomMessage("Match with this id does not exist")));
        }

        return Task.FromResult(new ServiceResult<string>(snapshot.SerializeEntries()));
    }
}
