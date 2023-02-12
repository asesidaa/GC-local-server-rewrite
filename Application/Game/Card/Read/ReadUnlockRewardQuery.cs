using Application.Common.Extensions;
using Application.Common.Models;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Game.Card.Read;


public record ReadUnlockRewardQuery(long CardId) : IRequestWrapper<string>;

public class ReadUnlockRewardQueryHandler : CardRequestHandlerBase<ReadUnlockRewardQuery, string>
{
    public ReadUnlockRewardQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override Task<ServiceResult<string>> Handle(ReadUnlockRewardQuery request, CancellationToken cancellationToken)
    {
      throw new NotImplementedException();  
    }
}
