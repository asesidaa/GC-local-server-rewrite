using Application.Common.Helpers;
using Domain.Enums;
using Shared.Dto.Api;

namespace Application.Api;

public record GetDefaultUnlockBitsetQuery(UnlockItemType ItemType) : IRequestWrapper<DefaultUnlockDetailDto>;

public class GetDefaultUnlockBitsetQueryHandler : RequestHandlerBase<GetDefaultUnlockBitsetQuery, DefaultUnlockDetailDto>
{
    public GetDefaultUnlockBitsetQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override async Task<ServiceResult<DefaultUnlockDetailDto>> Handle(GetDefaultUnlockBitsetQuery request,
        CancellationToken cancellationToken)
    {
        var itemCount = await GetItemCountAsync(request.ItemType, cancellationToken);
        var typeInt = (int)request.ItemType;

        var state = await CardDbContext.DefaultUnlockStates
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ItemType == typeInt, cancellationToken);

        var bitset = state is not null
            ? BitsetHelper.EnsureLength(BitsetHelper.Deserialize(state.UnlockedBitset), itemCount)
            : BitsetHelper.CreateAllZeroes(itemCount);

        return new ServiceResult<DefaultUnlockDetailDto>(new DefaultUnlockDetailDto
        {
            ItemType = request.ItemType.ToString(),
            TotalCount = itemCount,
            Bitset = bitset
        });
    }
}
