using Application.Common.Extensions;
using Application.Common.Models;
using Application.Dto;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Game.Card.Read;


public record ReadTitleQuery(long CardId) : IRequestWrapper<string>;

public class ReadTitleQueryHandler : CardRequestHandlerBase<ReadTitleQuery, string>
{
    private const string TITLE_XPATH = "/root/title/record";
    
    public ReadTitleQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override Task<ServiceResult<string>> Handle(ReadTitleQuery request, CancellationToken cancellationToken)
    {
        var count = Config.TitleCount;
        
        var list = new List<TitleDto>();
        for (int i = 0; i < count; i++)
        {
            var soundEffect = new TitleDto
            {
                Id = i,
                CardId = request.CardId,
                TitleId = i,
                Created = "2013-01-01 08:00:00",
                Modified = "2013-01-01 08:00:00",
                NewFlag = 0,
                UseFlag = 1
            };
            list.Add(soundEffect);
        }

        var result = list.SerializeCardDataList(TITLE_XPATH);

        return Task.FromResult(new ServiceResult<string>(result));  
    }
}
