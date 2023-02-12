using Application.Common.Extensions;
using Application.Common.Models;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Game.Card.Read;


public record ReadCardBDataQuery(long CardId, string Data) : IRequestWrapper<string>;

public class ReadCardBDataQueryHandler : CardRequestHandlerBase<ReadCardBDataQuery, string>
{
    public ReadCardBDataQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override Task<ServiceResult<string>> Handle(ReadCardBDataQuery request, CancellationToken cancellationToken)
    {
      throw new NotImplementedException();  
    }
}
