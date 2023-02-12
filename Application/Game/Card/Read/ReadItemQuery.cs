using Application.Common.Extensions;
using Application.Common.Models;
using Application.Dto;
using Application.Interfaces;
using Domain.Config;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Application.Game.Card.Read;

public record ReadItemQuery(long CardId) : IRequestWrapper<string>;

public class ReadItemQueryHandler : CardRequestHandlerBase<ReadItemQuery, string>
{
    private const string ITEM_XPATH = "/root/item/record";
    
    private readonly GameConfig config;
    
    public ReadItemQueryHandler(ICardDependencyAggregate aggregate, IOptions<GameConfig> options) : base(aggregate)
    {
        config = options.Value;
    }

    public override Task<ServiceResult<string>> Handle(ReadItemQuery request, CancellationToken cancellationToken)
    {
        var count = config.AvatarCount;
        var list = new List<ItemDto>();
        for (int i = 0; i < count; i++)
        {
            var avatar = new ItemDto()
            {
                Id = i,
                CardId = request.CardId,
                ItemId = i,
                ItemNum = 90,
                Created = "2013-01-01",
                Modified = "2013-01-01",
                NewFlag = 0,
                UseFlag = 1
            };
            list.Add(avatar);
        }

        var result = list.SerializeCardDataList(ITEM_XPATH);

        return Task.FromResult(new ServiceResult<string>(result));
    }
}