using Microsoft.Extensions.Logging;

namespace Application.Game.Card.OnlineMatching;

public record UpdateOnlineMatchingCommand(long CardId, string Data) : ICommandWrapper<string>;

public class UpdateOnlineMatchingCommandHandler : ICommandHandlerWrapper<UpdateOnlineMatchingCommand, string>
{
    private readonly IOnlineMatchingService matchingService;

    private readonly ILogger<UpdateOnlineMatchingCommandHandler> logger;

    public UpdateOnlineMatchingCommandHandler(IOnlineMatchingService matchingService,
        ILogger<UpdateOnlineMatchingCommandHandler> logger)
    {
        this.matchingService = matchingService;
        this.logger = logger;
    }

    public ValueTask<ServiceResult<string>> Handle(UpdateOnlineMatchingCommand request, CancellationToken cancellationToken)
    {
        var data = request.Data.DeserializeCardData<OnlineMatchEntryDto>();

        var snapshot = matchingService.UpdateRoom(data.MatchId, request.CardId, data.MessageId);
        if (snapshot is null)
        {
            logger.LogWarning("Match id {MatchId} not found", data.MatchId);
            return ValueTask.FromResult(
                ServiceResult.Failed<string>(ServiceError.CustomMessage("Match with this id does not exist")));
        }

        return ValueTask.FromResult(new ServiceResult<string>(snapshot.SerializeEntries()));
    }
}
