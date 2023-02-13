using System.Xml.Serialization;
using Application.Common.Extensions;
using Application.Common.Models;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Game.Card.Read;


public record ReadUnlockRewardQuery(long CardId) : IRequestWrapper<string>;

public class ReadUnlockRewardQueryHandler : CardRequestHandlerBase<ReadUnlockRewardQuery, string>
{
    private const string UNLOCK_REWARD_XPATH = "/root/unlock_reward/record";
    
    public ReadUnlockRewardQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override Task<ServiceResult<string>> Handle(ReadUnlockRewardQuery request, CancellationToken cancellationToken)
    {
        var config = Config.UnlockRewards;
        var list = config.Select((rewardConfig, i) => new UnlockRewardModel
        {
            Id = i,
            CardId = request.CardId,
            RewardId = rewardConfig.RewardId,
            RewardType = rewardConfig.RewardType,
            TargetId = rewardConfig.TargetId,
            TargetNum = rewardConfig.TargetNum,
            KeyNum = rewardConfig.KeyNum,
            DisplayFlag = 1,
            UseFlag = 1,
            LimitedFlag = 0,
            Created = "2013-01-01 08:00:00",
            Modified = "2013-01-01 08:00:00",
            OpenDate = "2013-01-01",
            CloseDate = "2030-01-01",
            OpenTime = "00:00:01",
            CloseTime = "23:59:59",
            OpenUnixTime = new DateTimeOffset(2013, 1, 1, 0, 0, 1, TimeSpan.Zero).ToUnixTimeSeconds(),
            CloseUnixTime = new DateTimeOffset(2030, 1, 1, 23, 59, 59, TimeSpan.Zero).ToUnixTimeSeconds()
        });

        var result = list.SerializeCardDataList(UNLOCK_REWARD_XPATH);

        return Task.FromResult(new ServiceResult<string>(result));
    }
}

public class UnlockRewardModel
{
    [XmlAttribute(AttributeName = "id")]
    public int Id { get; set; }
    
    [XmlElement(ElementName = "card_id")]
    public long CardId { get; set; }

    [XmlElement(ElementName = "reward_id")]
    public int RewardId { get; set; }

    [XmlElement(ElementName = "reward_type")]
    public int RewardType { get; set; }

    [XmlElement(ElementName = "open_date")]
    public string OpenDate { get; set; } = "2013-01-01";

    [XmlElement(ElementName = "close_date")]
    public string CloseDate { get; set; } = "2030-01-01";

    [XmlElement(ElementName = "open_time")]
    public string OpenTime { get; set; } = "00:00:01";

    [XmlElement(ElementName = "close_time")]
    public string CloseTime { get; set; } = "23:59:59";

    [XmlElement(ElementName = "target_id")]
    public int TargetId { get; set; }

    [XmlElement(ElementName = "target_num")]
    public int TargetNum { get; set; }

    [XmlElement(ElementName = "key_num")]
    public int KeyNum { get; set; }

    [XmlElement("display_flag")]
    public int DisplayFlag { get; set; }

    [XmlElement("use_flag")]
    public int UseFlag { get; set; } 

    [XmlElement("limited_flag")]
    public int LimitedFlag { get; set; }

    [XmlElement("open_unixtime")]
    public long OpenUnixTime { get; set; } 

    [XmlElement("close_unixtime")]
    public long CloseUnixTime { get; set; }

    [XmlElement("created")] 
    public string Created { get; set; } = string.Empty;

    [XmlElement("modified")]
    public string Modified { get; set; } = string.Empty;
}
