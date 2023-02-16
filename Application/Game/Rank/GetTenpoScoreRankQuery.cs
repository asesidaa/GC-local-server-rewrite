using System.Xml.Serialization;
using Application.Common.Extensions;
using Application.Common.Helpers;
using Application.Common.Models;
using Application.Dto;
using Application.Interfaces;
using Application.Mappers;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Game.Rank;

public record GetTenpoScoreRankQuery(int TenpoId) : IRequestWrapper<string>;

public class GetTenpoScoreRankQueryHandler : IRequestHandlerWrapper<GetTenpoScoreRankQuery, string>
{
    private readonly ICardDbContext cardDbContext;

    public GetTenpoScoreRankQueryHandler(ICardDbContext cardDbContext)
    {
        this.cardDbContext = cardDbContext;
    }

    public async Task<ServiceResult<string>> Handle(GetTenpoScoreRankQuery request, CancellationToken cancellationToken)
    {
        var ranks = await cardDbContext.GlobalScoreRanks.Where(rank => rank.LastPlayTenpoId == request.TenpoId)
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
