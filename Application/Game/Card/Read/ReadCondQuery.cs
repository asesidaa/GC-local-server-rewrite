using Application.Common.Extensions;
using Application.Common.Models;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Game.Card.Read;


public record ReadCondQuery(long CardId, string Data) : IRequestWrapper<string>;

public class ReadCondQueryHandler : CardRequestHandlerBase<ReadCondQuery, string>
{
    public ReadCondQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override Task<ServiceResult<string>> Handle(ReadCondQuery request, CancellationToken cancellationToken)
    {
      throw new NotImplementedException();  
    }
}
