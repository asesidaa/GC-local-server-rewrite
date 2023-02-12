using Application.Common.Extensions;
using Application.Common.Models;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Game.Card.Read;


public record ReadNavigatorQuery(long CardId, string Data) : IRequestWrapper<string>;

public class ReadNavigatorQueryHandler : CardRequestHandlerBase<ReadNavigatorQuery, string>
{
    public ReadNavigatorQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override Task<ServiceResult<string>> Handle(ReadNavigatorQuery request, CancellationToken cancellationToken)
    {
      throw new NotImplementedException();  
    }
}
