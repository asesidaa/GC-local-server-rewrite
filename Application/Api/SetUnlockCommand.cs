using Application.Common.Helpers;
using Domain.Entities;
using Domain.Enums;

namespace Application.Api;

public record SetUnlockCommand(long CardId, UnlockItemType ItemType, int ItemId, bool Unlock) : IRequestWrapper<bool>;

public class SetUnlockCommandHandler : RequestHandlerBase<SetUnlockCommand, bool>
{
    public SetUnlockCommandHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override async Task<ServiceResult<bool>> Handle(SetUnlockCommand request, CancellationToken cancellationToken)
    {
        var typeInt = (int)request.ItemType;
        var itemCount = await GetItemCountAsync(request.ItemType, cancellationToken);

        if (request.ItemId < 1 || request.ItemId > itemCount)
            return ServiceResult.Failed<bool>(ServiceError.CustomMessage($"Item ID {request.ItemId} is out of range for {request.ItemType}"));

        var state = await CardDbContext.PlayerUnlockStates
            .FirstOrDefaultAsync(s => s.CardId == request.CardId && s.ItemType == typeInt, cancellationToken);

        long[] bitset;
        if (state is not null)
        {
            bitset = BitsetHelper.EnsureLength(BitsetHelper.Deserialize(state.UnlockedBitset), itemCount);
        }
        else
        {
            var defaultState = await CardDbContext.DefaultUnlockStates
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ItemType == typeInt, cancellationToken);

            bitset = defaultState is not null
                ? BitsetHelper.EnsureLength(BitsetHelper.Deserialize(defaultState.UnlockedBitset), itemCount)
                : BitsetHelper.CreateAllZeroes(itemCount);

            state = new PlayerUnlockState
            {
                CardId = request.CardId,
                ItemType = typeInt
            };
            CardDbContext.PlayerUnlockStates.Add(state);
        }

        BitsetHelper.SetBit(bitset, request.ItemId, request.Unlock);
        state.UnlockedBitset = BitsetHelper.Serialize(bitset);

        await CardDbContext.SaveChangesAsync(cancellationToken);

        return new ServiceResult<bool>(true);
    }
}
