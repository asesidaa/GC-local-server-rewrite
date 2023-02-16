using Application.Common.Helpers;

namespace Application.Game.Rank;

public record GetTenpoScoreRankQuery(int TenpoId, string Param) : IRequestWrapper<string>;

public class GetTenpoScoreRankQueryHandler : IRequestHandlerWrapper<GetTenpoScoreRankQuery, string>
{
    private readonly ICardDbContext cardDbContext;

    public GetTenpoScoreRankQueryHandler(ICardDbContext cardDbContext)
    {
        this.cardDbContext = cardDbContext;
    }

    public async Task<ServiceResult<string>> Handle(GetTenpoScoreRankQuery request, CancellationToken cancellationToken)
    {
        var param = request.Param.DeserializeCardData<RankParam>();
        if (param.CardId == 0)
        {
            return await GetAllRanks(request.TenpoId, cancellationToken);
        }
        return await GetCardRank(param.CardId, request.TenpoId, cancellationToken);
    }

    private async Task<ServiceResult<string>> GetCardRank(long cardId, int tenpoId, CancellationToken cancellationToken)
    {
        var rank = await cardDbContext.GlobalScoreRanks.FirstOrDefaultAsync(scoreRank => scoreRank.CardId == cardId &&
            scoreRank.LastPlayTenpoId == tenpoId, 
            cancellationToken: cancellationToken);
        var container = new TenpoScoreRankContainer
        {
            Ranks = new List<ScoreRankDto>(),
            Status = new RankStatus
            {
                TableName = "TenpoScoreRank",
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

    private async Task<ServiceResult<string>> GetAllRanks(int tenpoId, CancellationToken cancellationToken)
    {
        var ranks = await cardDbContext.GlobalScoreRanks.Where(rank => rank.LastPlayTenpoId == tenpoId)
            .OrderByDescending(rank => rank.TotalScore)
            .Take(30)
            .ToListAsync(cancellationToken: cancellationToken);
        ranks = ranks.Select((rank, i) =>
        {
            rank.Rank = i + 1;
            return rank;
        }).ToList();

        var dtoList = ranks.Select((rank, i) =>
        {
            var dto = rank.ScoreRankToDto();
            dto.Id = i;
            dto.Rank2 = dto.Rank;
            return dto;
        }).ToList();

        var container = new TenpoScoreRankContainer
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
}

[XmlRoot("root")]
public class TenpoScoreRankContainer
{
    [XmlArray(ElementName = "t_score_rank")]
    [XmlArrayItem(ElementName = "record")]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public List<ScoreRankDto> Ranks { get; init; } = new();

    [XmlElement("ranking_status")] 
    public RankStatus Status { get; set; } = new();
}
