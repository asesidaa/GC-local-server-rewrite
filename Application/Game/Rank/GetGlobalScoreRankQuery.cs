using System.Xml.Serialization;
using Application.Common.Extensions;
using Application.Common.Helpers;
using Application.Common.Models;
using Application.Interfaces;

namespace Application.Game.Rank;

public record GetGlobalScoreRankQuery() : IRequestWrapper<string>;

public class GetGlobalScoreRankQueryHandler : IRequestHandlerWrapper<GetGlobalScoreRankQuery, string>
{
    public Task<ServiceResult<string>> Handle(GetGlobalScoreRankQuery request, CancellationToken cancellationToken)
    {
        var container = new GlobalScoreRankContainer
        {
            Ranks = new List<object>(),
            Status = new RankStatus
            {
                TableName = "TenpoScoreRank",
                StartDate = TimeHelper.DateToString(DateTime.Today),
                EndDate = TimeHelper.DateToString(DateTime.Today),
                Rows = 0,
                Status = 0
            }
        };

        return Task.FromResult(new ServiceResult<string>(container.SerializeCardData()));
    }
}

[XmlRoot("root")]
public class GlobalScoreRankContainer
{
    [XmlArray(ElementName = "score_rank")]
    [XmlArrayItem(ElementName = "record")]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public List<object> Ranks { get; init; } = new();

    [XmlElement("ranking_status")] 
    public RankStatus Status { get; set; } = new();
}

