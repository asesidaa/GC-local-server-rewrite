using Domain.Entities;
using Shared.Dto.Api;

namespace Application.Api;

public record SetPlayerCoinsCommand(PlayerCoinDto Coins) : IRequestWrapper<bool>;

public class SetPlayerCoinsCommandHandler : RequestHandlerBase<SetPlayerCoinsCommand, bool>
{
    public SetPlayerCoinsCommandHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override async Task<ServiceResult<bool>> Handle(SetPlayerCoinsCommand request, CancellationToken cancellationToken)
    {
        var coin = await CardDbContext.PlayerCoins
            .FirstOrDefaultAsync(c => c.CardId == request.Coins.CardId, cancellationToken);

        if (coin is null)
        {
            coin = new PlayerCoin { CardId = request.Coins.CardId };
            CardDbContext.PlayerCoins.Add(coin);
        }

        coin.CurrentCoins = request.Coins.CurrentCoins;
        coin.TotalCoins = request.Coins.TotalCoins;
        coin.MonthlyCoins = request.Coins.MonthlyCoins;

        await CardDbContext.SaveChangesAsync(cancellationToken);

        return new ServiceResult<bool>(true);
    }
}
