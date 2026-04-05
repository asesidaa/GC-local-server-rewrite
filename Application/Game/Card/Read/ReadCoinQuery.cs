namespace Application.Game.Card.Read;


public record ReadCoinQuery(long CardId) : IRequestWrapper<string>;

public class ReadCoinQueryHandler : RequestHandlerBase<ReadCoinQuery, string>
{
    private const string COIN_XPATH = "/root/coin";

    public ReadCoinQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override async Task<ServiceResult<string>> Handle(ReadCoinQuery request, CancellationToken cancellationToken)
    {
        var dto = new CoinDto
        {
            CardId = request.CardId,
            Created = "2013-01-01 08:00:00",
            Modified = "2013-01-01 08:00:00"
        };

        if (UnlockConfig.EnableLockSystem)
        {
            var coin = await CardDbContext.PlayerCoins
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CardId == request.CardId, cancellationToken);

            if (coin is not null)
            {
                dto.CurrentCoins = coin.CurrentCoins;
                dto.TotalCoins = coin.TotalCoins;
                dto.MonthlyCoins = coin.MonthlyCoins;
            }
            else
            {
                dto.CurrentCoins = UnlockConfig.DefaultCoins;
                dto.TotalCoins = UnlockConfig.DefaultCoins;
                dto.MonthlyCoins = UnlockConfig.DefaultCoins;
            }
        }
        else
        {
            dto.CurrentCoins = UnlockConfig.DefaultCoins;
            dto.TotalCoins = UnlockConfig.DefaultCoins;
            dto.MonthlyCoins = UnlockConfig.DefaultCoins;
        }

        var result = dto.SerializeCardData(COIN_XPATH);

        return new ServiceResult<string>(result);
    }
}
