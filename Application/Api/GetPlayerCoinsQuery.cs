using Shared.Dto.Api;

namespace Application.Api;

public record GetPlayerCoinsQuery(long CardId) : IRequestWrapper<PlayerCoinDto>;

public class GetPlayerCoinsQueryHandler : RequestHandlerBase<GetPlayerCoinsQuery, PlayerCoinDto>
{
    public GetPlayerCoinsQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override async Task<ServiceResult<PlayerCoinDto>> Handle(GetPlayerCoinsQuery request,
        CancellationToken cancellationToken)
    {
        var coin = await CardDbContext.PlayerCoins
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CardId == request.CardId, cancellationToken);

        var dto = new PlayerCoinDto
        {
            CardId = request.CardId,
            CurrentCoins = coin?.CurrentCoins ?? UnlockConfig.DefaultCoins,
            TotalCoins = coin?.TotalCoins ?? UnlockConfig.DefaultCoins,
            MonthlyCoins = coin?.MonthlyCoins ?? UnlockConfig.DefaultCoins
        };

        return new ServiceResult<PlayerCoinDto>(dto);
    }
}
