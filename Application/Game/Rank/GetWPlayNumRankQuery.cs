using System.Diagnostics;
using Application.Common.Helpers;

namespace Application.Game.Rank;

public record GetWPlayNumRankQuery(): IRequestWrapper<string>;

public class GetWPlayNumRankQueryHandler : IRequestHandlerWrapper<GetWPlayNumRankQuery, string>
{
    private readonly ICardDbContext cardDbContext;

    public GetWPlayNumRankQueryHandler(ICardDbContext cardDbContext)
    {
        this.cardDbContext = cardDbContext;
    }
    
    public async Task<ServiceResult<string>> Handle(GetWPlayNumRankQuery request, CancellationToken cancellationToken)
    {
        var ranks = await cardDbContext.PlayNumRanks.OrderBy(rank => rank.Rank)
            .Take(30).ToListAsync(cancellationToken: cancellationToken);

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
            return dto;
        }).ToList();

        var container = new WPlayNumRankContainer
        {
            Ranks = dtoList,
            Status = status
        };

        var result = container.SerializeCardData();

        return new ServiceResult<string>(result);
    }
}

[XmlRoot("root")]
public class WPlayNumRankContainer
{
    [XmlArray(ElementName = "w_play_num_rank")]
    [XmlArrayItem(ElementName = "record")]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public List<PlayNumRankDto> Ranks { get; init; } = new();

    [XmlElement("ranking_status")] 
    public RankStatus Status { get; set; } = new();
}

