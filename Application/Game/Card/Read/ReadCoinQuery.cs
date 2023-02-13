using Application.Common.Extensions;
using Application.Common.Models;
using Application.Dto;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Game.Card.Read;


public record ReadCoinQuery(long CardId) : IRequestWrapper<string>;

public class ReadCoinQueryHandler : CardRequestHandlerBase<ReadCoinQuery, string>
{
    private const string COIN_XPATH = "/root/coin";
    
    public ReadCoinQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override Task<ServiceResult<string>> Handle(ReadCoinQuery request, CancellationToken cancellationToken)
    {
        var dto = new CoinDto
        {
            CardId = request.CardId,
            CurrentCoins = 900000,
            MonthlyCoins = 900000,
            TotalCoins = 900000,
            Created = "2013-01-01 08:00:00",
            Modified = "2013-01-01 08:00:00"
        };

        var result = dto.SerializeCardData(COIN_XPATH);

        return Task.FromResult(new ServiceResult<string>(result));
    }
}
