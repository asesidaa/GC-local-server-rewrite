using System.Diagnostics;
using System.Xml.Serialization;
using Application.Common.Extensions;
using Application.Common.Helpers;
using Application.Common.Models;
using Application.Dto;
using Application.Interfaces;
using Application.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Application.Game.Rank;

public record GetPlayNumRankQuery(): IRequestWrapper<string>;

public class GetPlayNumRankQueryHandler : IRequestHandlerWrapper<GetPlayNumRankQuery, string>
{
    private readonly ICardDbContext cardDbContext;

    public GetPlayNumRankQueryHandler(ICardDbContext cardDbContext)
    {
        this.cardDbContext = cardDbContext;
    }
    
    public async Task<ServiceResult<string>> Handle(GetPlayNumRankQuery request, CancellationToken cancellationToken)
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

