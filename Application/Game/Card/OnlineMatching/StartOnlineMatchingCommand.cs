using Application.Common.Helpers;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;

namespace Application.Game.Card.OnlineMatching;

public record StartOnlineMatchingCommand(long CardId, string Data) : IRequestWrapper<string>;

public class StartOnlineMatchingCommandHandler : RequestHandlerBase<StartOnlineMatchingCommand, string>
{
    private const int MAX_RETRY = 5;
    
    private const string MATCH_ENRTY_XPATH = "/root/online_matching";
    
    private const string RECORD_XPATH       = $"{MATCH_ENRTY_XPATH}/record";

    private readonly ILogger<StartOnlineMatchingCommandHandler> logger;

    public StartOnlineMatchingCommandHandler(ICardDependencyAggregate aggregate, ILogger<StartOnlineMatchingCommandHandler> logger) : base(aggregate)
    {
        this.logger = logger;
    }

    public override async Task<ServiceResult<string>> Handle(StartOnlineMatchingCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Data.DeserializeCardData<OnlineMatchEntryDto>();
        dto.CardId = request.CardId;
        dto.StartTime = TimeHelper.CurrentTimeToString();
        dto.MatchTimeout = 20;
        dto.MatchRemainingTime = 5;
        dto.MatchWaitTime = 5;
        dto.Status = 1;
        var entry = dto.DtoToOnlineMatchEntry();

        var matchId = await CardDbContext.OnlineMatches.CountAsync(cancellationToken);
        for (int i = 0; i < MAX_RETRY; i++)
        {
            try
            {
                var onlineMatch = await CardDbContext.OnlineMatches
                    .Include(match => match.Entries)
                    .FirstOrDefaultAsync(match => match.IsOpen && match.Entries.Count < 4, cancellationToken);
                string result;
                if (onlineMatch is not null)
                {
                    entry.EntryId = onlineMatch.Entries.Count;
                    onlineMatch.Entries.Add(entry);
                    onlineMatch.Guid = Guid.NewGuid();
                    await CardDbContext.SaveChangesAsync(cancellationToken);
                    result = onlineMatch.Entries.Select((matchEntry, id) =>
                    {
                        var entryDto = matchEntry.OnlineMatchEntryToDto();
                        entryDto.Id = id;
                        return entryDto;
                    }).SerializeCardDataList(RECORD_XPATH);
                    return new ServiceResult<string>(result);
                }

                entry.EntryId = 0;
                onlineMatch = new OnlineMatch
                {
                    MatchId = matchId,
                    Entries = { entry },
                    Guid = Guid.NewGuid(),
                    IsOpen = true
                };
                CardDbContext.OnlineMatches.Add(onlineMatch);
                await CardDbContext.SaveChangesAsync(cancellationToken);
                result = onlineMatch.Entries.Select((matchEntry, id) =>
                {
                    var entryDto = matchEntry.OnlineMatchEntryToDto();
                    entryDto.Id = id;
                    return entryDto;
                }).SerializeCardDataList(RECORD_XPATH);
                return new ServiceResult<string>(result);
            }
            catch (DbUpdateConcurrencyException e)
            {
                logger.LogWarning(e, "Concurrent DB update when starting online match");
            }
            catch (DbUpdateException e)
                when (e.InnerException != null
                      && e.InnerException.Message.StartsWith("Cannot insert duplicate key row in object"))
            {
                logger.LogWarning(e, "Concurrent insert when starting online match");
            }
        }
        logger.LogError("Cannot update DB after {Number} trials for online match!", MAX_RETRY);
        return ServiceResult.Failed<string>(ServiceError.DatabaseSaveFailed);
    }
}