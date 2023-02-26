using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Application.Game.Card.OnlineMatching;

public record UpdateOnlineMatchingCommand(long CardId, string Data) : IRequestWrapper<string>;

public class UpdateOnlineMatchingCommandHandler : RequestHandlerBase<UpdateOnlineMatchingCommand, string>
{
    private readonly ILogger<UpdateOnlineMatchingCommandHandler> logger;

    private const string MATCH_ENRTY_XPATH = "/root/online_matching";

    private const string RECORD_XPATH = $"{MATCH_ENRTY_XPATH}/record";

    private const int MAX_RETRY = 5;

    public UpdateOnlineMatchingCommandHandler(ICardDependencyAggregate aggregate,
        ILogger<UpdateOnlineMatchingCommandHandler>                    logger) : base(aggregate)
    {
        this.logger = logger;
    }

    [SuppressMessage("ReSharper.DPA", "DPA0006: Large number of DB commands")]
    public override async Task<ServiceResult<string>> Handle(UpdateOnlineMatchingCommand request,
        CancellationToken                                                                cancellationToken)
    {
        var data = request.Data.DeserializeCardData<OnlineMatchEntryDto>();
        for (int i = 0; i < MAX_RETRY; i++)
        {
            try
            {
                var match = await CardDbContext.OnlineMatches
                    .Include(onlineMatch => onlineMatch.Entries)
                    .FirstOrDefaultAsync(onlineMatch =>
                        onlineMatch.MatchId == data.MatchId, cancellationToken);
                if (match is null)
                {
                    logger.LogWarning("Match id {MatchId} not found", data.MatchId);
                    return ServiceResult.Failed<string>(ServiceError.CustomMessage("Match with this id does not exist"));
                }

                match.Entries.ForEach(entry =>
                {
                    if (entry.CardId == request.CardId)
                    {
                        entry.MessageId = data.MessageId;
                    }

                    entry.MatchRemainingTime--;
                });

                if (match.Entries.TrueForAll(entry => entry.MatchRemainingTime <= 0))
                {
                    match.Entries.ForEach(entry => entry.Status = 3);
                    match.IsOpen = false;
                }

                match.Guid = Guid.NewGuid();

                await CardDbContext.SaveChangesAsync(cancellationToken);
                var result = match.Entries.Select((matchEntry, id) =>
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
        }
        logger.LogError("Cannot update DB after {Number} trials for online match!", MAX_RETRY);
        return ServiceResult.Failed<string>(ServiceError.DatabaseSaveFailed); 
    }
}