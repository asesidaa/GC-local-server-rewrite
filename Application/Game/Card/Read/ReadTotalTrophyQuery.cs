using Application.Common.Extensions;
using Application.Common.Models;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Game.Card.Read;


public record ReadTotalTrophyQuery(long CardId, string Data) : IRequestWrapper<string>;

public class ReadTotalTrophyQueryHandler : CardRequestHandlerBase<ReadTotalTrophyQuery, string>
{
    public ReadTotalTrophyQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override Task<ServiceResult<string>> Handle(ReadTotalTrophyQuery request, CancellationToken cancellationToken)
    {
      throw new NotImplementedException();  
    }
}
