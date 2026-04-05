using Application.Common.Helpers;
using Domain.Entities;
using Domain.Enums;

namespace Application.Api;

public record SetUnlockAllCommand(long CardId, UnlockItemType ItemType, bool Unlock) : IRequestWrapper<bool>;

public class SetUnlockAllCommandHandler : RequestHandlerBase<SetUnlockAllCommand, bool>
{
    public SetUnlockAllCommandHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override async Task<ServiceResult<bool>> Handle(SetUnlockAllCommand request, CancellationToken cancellationToken)
    {
        var typeInt = (int)request.ItemType;
        var itemCount = await GetItemCountAsync(request.ItemType, cancellationToken);

        var state = await CardDbContext.PlayerUnlockStates
            .FirstOrDefaultAsync(s => s.CardId == request.CardId && s.ItemType == typeInt, cancellationToken);

        if (state is null)
        {
            state = new PlayerUnlockState
            {
                CardId = request.CardId,
                ItemType = typeInt
            };
            CardDbContext.PlayerUnlockStates.Add(state);
        }

        var bitset = request.Unlock
            ? BitsetHelper.CreateAllOnes(itemCount)
            : BitsetHelper.CreateAllZeroes(itemCount);

        state.UnlockedBitset = BitsetHelper.Serialize(bitset);

        await CardDbContext.SaveChangesAsync(cancellationToken);

        return new ServiceResult<bool>(true);
    }
}
