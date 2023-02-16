using System.Xml.Serialization;
using Application.Common.Extensions;
using Application.Common.Helpers;
using Application.Common.Models;
using Application.Dto;
using Application.Interfaces;
using Application.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Application.Game.Rank;

public record GetGlobalScoreRankQuery() : IRequestWrapper<string>;

public class GetGlobalScoreRankQueryHandler : IRequestHandlerWrapper<GetGlobalScoreRankQuery, string>
{
    private readonly ICardDbContext cardDbContext;

    public GetGlobalScoreRankQueryHandler(ICardDbContext cardDbContext)
    {
        this.cardDbContext = cardDbContext;
    }

    public async Task<ServiceResult<string>> Handle(GetGlobalScoreRankQuery request, CancellationToken cancellationToken)
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
        
        var container = new GlobalScoreRankContainer
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
public class GlobalScoreRankContainer
{
    [XmlArray(ElementName = "score_rank")]
    [XmlArrayItem(ElementName = "record")]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public List<ScoreRankDto> Ranks { get; init; } = new();

    [XmlElement("ranking_status")] 
    public RankStatus Status { get; set; } = new();
}

