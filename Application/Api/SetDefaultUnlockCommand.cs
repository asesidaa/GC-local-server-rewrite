using Application.Common.Helpers;
using Domain.Entities;
using Domain.Enums;

namespace Application.Api;

public record SetDefaultUnlockCommand(UnlockItemType ItemType, int ItemId, bool Unlock) : IRequestWrapper<bool>;

public class SetDefaultUnlockCommandHandler : RequestHandlerBase<SetDefaultUnlockCommand, bool>
{
    public SetDefaultUnlockCommandHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override async Task<ServiceResult<bool>> Handle(SetDefaultUnlockCommand request, CancellationToken cancellationToken)
    {
        var typeInt = (int)request.ItemType;
        var itemCount = await GetItemCountAsync(request.ItemType, cancellationToken);

        if (request.ItemId < 1 || request.ItemId > itemCount)
            return ServiceResult.Failed<bool>(ServiceError.CustomMessage($"Item ID {request.ItemId} is out of range for {request.ItemType}"));

        var state = await CardDbContext.DefaultUnlockStates
            .FirstOrDefaultAsync(s => s.ItemType == typeInt, cancellationToken);

        long[] bitset;
        if (state is not null)
        {
            bitset = BitsetHelper.EnsureLength(BitsetHelper.Deserialize(state.UnlockedBitset), itemCount);
        }
        else
        {
            bitset = BitsetHelper.CreateAllZeroes(itemCount);
            state = new DefaultUnlockState { ItemType = typeInt };
            CardDbContext.DefaultUnlockStates.Add(state);
        }

        BitsetHelper.SetBit(bitset, request.ItemId, request.Unlock);
        state.UnlockedBitset = BitsetHelper.Serialize(bitset);

        await CardDbContext.SaveChangesAsync(cancellationToken);

        return new ServiceResult<bool>(true);
    }
}
