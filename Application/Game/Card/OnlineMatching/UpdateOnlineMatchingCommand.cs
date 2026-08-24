using Microsoft.Extensions.Logging;

namespace Application.Game.Card.OnlineMatching;

public record UpdateOnlineMatchingCommand(long CardId, string Data) : ICommandWrapper<string>;

public sealed class UpdateOnlineMatchingCommandHandler : ICommandHandlerWrapper<UpdateOnlineMatchingCommand, string>
{
    private readonly IOnlineMatchingService matchingService;
    private readonly ILogger<UpdateOnlineMatchingCommandHandler> logger;

    public UpdateOnlineMatchingCommandHandler(
        IOnlineMatchingService matchingService,
        ILogger<UpdateOnlineMatchingCommandHandler> logger)
    {
        this.matchingService = matchingService;
        this.logger = logger;
    }

    public ValueTask<ServiceResult<string>> Handle(
        UpdateOnlineMatchingCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var dto = request.Data.DeserializeCardData<OnlineMatchingUpdateRequestDto>();
            var update = OnlineMatchingXml.ToUpdate(dto);
            var result = matchingService.Update(
                update.MatchingId,
                update.EventId,
                request.CardId,
                update.Action,
                update.MessageId);
            if (!result.IsSuccess)
            {
                logger.LogWarning(
                    "Rejected online match update for card {CardId}, match {MatchingId}: {Reason}",
                    request.CardId,
                    update.MatchingId,
                    result.Error);
                return ValueTask.FromResult(
                    ServiceResult.Failed<string>(ServiceError.CustomMessage(result.Message!)));
            }

            return ValueTask.FromResult(
                new ServiceResult<string>(OnlineMatchingXml.SerializeSnapshot(result.Value!)));
        }
        catch (OnlineMatchingProtocolException ex)
        {
            return ValueTask.FromResult(ServiceResult.Failed<string>(ServiceError.CustomMessage(ex.Message)));
        }
    }
}
