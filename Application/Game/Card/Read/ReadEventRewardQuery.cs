using Application.Common.Extensions;
using Application.Common.Models;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Game.Card.Read;


public record ReadEventRewardQuery(long CardId, string Data) : IRequestWrapper<string>;

public class ReadEventRewardQueryHandler : CardRequestHandlerBase<ReadEventRewardQuery, string>
{
    public ReadEventRewardQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override Task<ServiceResult<string>> Handle(ReadEventRewardQuery request, CancellationToken cancellationToken)
    {
      throw new NotImplementedException();  
    }
}
