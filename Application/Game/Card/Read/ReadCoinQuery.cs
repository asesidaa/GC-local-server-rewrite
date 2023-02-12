using Application.Common.Extensions;
using Application.Common.Models;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Game.Card.Read;


public record ReadCoinQuery(long CardId, string Data) : IRequestWrapper<string>;

public class ReadCoinQueryHandler : CardRequestHandlerBase<ReadCoinQuery, string>
{
    public ReadCoinQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override Task<ServiceResult<string>> Handle(ReadCoinQuery request, CancellationToken cancellationToken)
    {
      throw new NotImplementedException();  
    }
}
