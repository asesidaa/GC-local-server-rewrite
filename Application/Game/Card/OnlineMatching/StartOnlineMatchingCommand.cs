using Application.Common.Helpers;
using Application.Mappers;

namespace Application.Game.Card.OnlineMatching;

public record StartOnlineMatchingCommand(long CardId, string Data) : IRequestWrapper<string>;

public class StartOnlineMatchingCommandHandler : IRequestHandlerWrapper<StartOnlineMatchingCommand, string>
{
    private readonly IOnlineMatchingService matchingService;

    public StartOnlineMatchingCommandHandler(IOnlineMatchingService matchingService)
    {
        this.matchingService = matchingService;
    }

    public Task<ServiceResult<string>> Handle(StartOnlineMatchingCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Data.DeserializeCardData<OnlineMatchEntryDto>();
        dto.CardId = request.CardId;
        dto.StartTime = TimeHelper.CurrentTimeToString();
        dto.MatchTimeout = 20;
        dto.MatchRemainingTime = 5;
        dto.MatchWaitTime = 5;
        dto.Status = 1;

        var entry = dto.DtoToMatchEntry();
        var snapshot = matchingService.JoinOrCreateRoom(entry);

        return Task.FromResult(new ServiceResult<string>(snapshot.SerializeEntries()));
    }
}
