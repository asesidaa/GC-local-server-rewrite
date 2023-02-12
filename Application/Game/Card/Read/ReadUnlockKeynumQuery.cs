using Application.Common.Extensions;
using Application.Common.Models;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Game.Card.Read;


public record ReadUnlockKeynumQuery(long CardId, string Data) : IRequestWrapper<string>;

public class ReadUnlockKeynumQueryHandler : CardRequestHandlerBase<ReadUnlockKeynumQuery, string>
{
    public ReadUnlockKeynumQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override Task<ServiceResult<string>> Handle(ReadUnlockKeynumQuery request, CancellationToken cancellationToken)
    {
      throw new NotImplementedException();  
    }
}
