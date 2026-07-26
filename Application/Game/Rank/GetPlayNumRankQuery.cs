using System.Diagnostics;
using Application.Common.Helpers;

namespace Application.Game.Rank;

public record GetPlayNumRankQuery(): IQueryWrapper<string>;

public class GetPlayNumRankQueryHandler : IQueryHandlerWrapper<GetPlayNumRankQuery, string>
{
    private readonly ICardDbContext cardDbContext;

    public GetPlayNumRankQueryHandler(ICardDbContext cardDbContext)
    {
        this.cardDbContext = cardDbContext;
    }
    
    public async ValueTask<ServiceResult<string>> Handle(GetPlayNumRankQuery request, CancellationToken cancellationToken)
    {
        var orderedRanks = await cardDbContext.PlayNumRanks
            .OrderBy(rank => rank.Rank)
            .ToListAsync(cancellationToken: cancellationToken);
        var playedRanks = orderedRanks.Where(rank => rank.PlayCount > 0).ToList();
        var fillerRanks = orderedRanks.Where(rank => rank.PlayCount == 0).ToArray();

        // The client filters unavailable songs before requiring 30 entries.
        // Keep real rankings first, then provide randomized zero-play candidates.
        Random.Shared.Shuffle(fillerRanks);
        var ranks = playedRanks.Concat(fillerRanks).ToList();

        var status = new RankStatus
        {
            TableName = "PlayNumRank",
            StartDate = TimeHelper.DateToString(Process.GetCurrentProcess().StartTime.Date),
            EndDate = TimeHelper.DateToString(DateTime.Today),
            Rows = ranks.Count,
            Status = 1
        };

        var dtoList = ranks.Select((rank, i) =>
        {
            var dto = rank.PlayNumRankToDto();
            dto.Id = i;
            dto.Rank = i + 1;
            return dto;
        }).ToList();

        var container = new PlayNumRankContainer
        {
            Ranks = dtoList,
            Status = status
        };

        var result = container.SerializeCardData();

        return new ServiceResult<string>(result);
    }
}

[XmlRoot("root")]
public class PlayNumRankContainer
{
    [XmlArray(ElementName = "play_num_rank")]
    [XmlArrayItem(ElementName = "record")]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public List<PlayNumRankDto> Ranks { get; init; } = new();

    [XmlElement("ranking_status")] 
    public RankStatus Status { get; set; } = new();
}

