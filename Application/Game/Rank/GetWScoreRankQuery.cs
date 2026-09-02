using Application.Common.Helpers;

namespace Application.Game.Rank;

public record GetWScoreRankQuery(string Param) : IQueryWrapper<string>;

public class GetWScoreRankQueryHandler : IQueryHandlerWrapper<GetWScoreRankQuery, string>
{
    private readonly ICardDbContext cardDbContext;

    public GetWScoreRankQueryHandler(ICardDbContext cardDbContext)
    {
        this.cardDbContext = cardDbContext;
    }

    public async ValueTask<ServiceResult<string>> Handle(GetWScoreRankQuery request, CancellationToken cancellationToken)
    {
        var param = request.Param.DeserializeCardData<RankParam>();
        return param switch
        {
            { CardId: 0, TenpoId: 0 }   => await GetAllRanks(cancellationToken),
            { CardId: > 0, TenpoId: 0 } => await GetCardRank(param.CardId, cancellationToken),
            { CardId: 0, TenpoId: > 0 } => await GetTenpoRanks(param.TenpoId, cancellationToken),
            _                           => ServiceResult.Failed<string>(ServiceError.ValidationFormat)
        };
    }

    private async Task<ServiceResult<string>> GetCardRank(long cardId,
        CancellationToken cancellationToken)
    {
        var rank = await cardDbContext.GlobalScoreRanks.FirstOrDefaultAsync(scoreRank => scoreRank.CardId == cardId, 
            cancellationToken: cancellationToken);
        var container =  new WScoreRankContainer
        {
            Ranks = new List<ScoreRankDto>(),
            Status = new RankStatus
            {
                TableName = "GlobalScoreRank",
                StartDate = TimeHelper.DateToString(DateTime.Today),
                EndDate = TimeHelper.DateToString(DateTime.Today),
                Rows = 0,
                Status = 1
            }
        };
        if (rank is null)
        {
            return new ServiceResult<string>(container.SerializeCardData());
        }

        var dto = rank.ScoreRankToDto();
        dto.Id = 0;
        container.Ranks.Add(dto);
        container.Status.Rows++;
        return new ServiceResult<string>(container.SerializeCardData());
    }

    private async Task<ServiceResult<string>> GetTenpoRanks(int tenpoId, CancellationToken cancellationToken)
    {
        var ranks = await cardDbContext.GlobalScoreRanks
            .Where(rank => rank.LastPlayTenpoId == tenpoId)
            .OrderByDescending(rank => rank.TotalScore)
            .Take(30)
            .ToListAsync(cancellationToken: cancellationToken);

        var dtoList = ranks.Select((rank, i) =>
        {
            var dto = rank.ScoreRankToDto();
            dto.Id = i;
            dto.Rank = i + 1;
            dto.Rank2 = dto.Rank;
            return dto;
        }).ToList();

        var container = new WScoreRankContainer
        {
            Ranks = dtoList,
            Status = new RankStatus
            {
                TableName = "TenpoScoreRank",
                StartDate = TimeHelper.DateToString(DateTime.Today),
                EndDate = TimeHelper.DateToString(DateTime.Today),
                Rows = dtoList.Count,
                Status = 1
            }
        };

        return new ServiceResult<string>(container.SerializeCardData());
    }

    private async Task<ServiceResult<string>> GetAllRanks(CancellationToken cancellationToken)
    {
        var ranks = await cardDbContext.GlobalScoreRanks.OrderBy(rank => rank.Rank)
            .Take(30).ToListAsync(cancellationToken: cancellationToken);

        var dtoList = ranks.Select((rank, i) =>
        {
            var dto = rank.ScoreRankToDto();
            dto.Id = i;
            dto.Rank2 = dto.Rank;
            return dto;
        }).ToList();

        var container = new WScoreRankContainer
        {
            Ranks = dtoList,
            Status = new RankStatus
            {
                TableName = "GlobalScoreRank",
                StartDate = TimeHelper.DateToString(DateTime.Today),
                EndDate = TimeHelper.DateToString(DateTime.Today),
                Rows = dtoList.Count,
                Status = 1
            }
        };

        return new ServiceResult<string>(container.SerializeCardData());
    }
}

[XmlRoot("root")]
public class WScoreRankContainer
{
    [XmlArray(ElementName = "w_score_rank")]
    [XmlArrayItem(ElementName = "record")]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public List<ScoreRankDto> Ranks { get; init; } = new();

    [XmlElement("ranking_status")] 
    public RankStatus Status { get; set; } = new();
}
