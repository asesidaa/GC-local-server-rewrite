using System.Xml.Serialization;
using Application.Common.Extensions;
using Application.Common.Helpers;
using Application.Common.Models;
using Application.Interfaces;

namespace Application.Game.Rank;

public record GetMonthlyScoreRankQuery() : IRequestWrapper<string>;

public class GetMonthlyScoreRankQueryHandler : IRequestHandlerWrapper<GetMonthlyScoreRankQuery, string>
{
    public Task<ServiceResult<string>> Handle(GetMonthlyScoreRankQuery request, CancellationToken cancellationToken)
    {
        var container = new MonthlyScoreRankContainer
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
public class MonthlyScoreRankContainer
{
    [XmlArray(ElementName = "m_score_rank")]
    [XmlArrayItem(ElementName = "record")]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public List<object> Ranks { get; init; } = new();

    [XmlElement("ranking_status")] 
    public RankStatus Status { get; set; } = new();
}

