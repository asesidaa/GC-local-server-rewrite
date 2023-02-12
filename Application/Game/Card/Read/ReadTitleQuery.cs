using Application.Common.Extensions;
using Application.Common.Models;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Game.Card.Read;


public record ReadTitleQuery(long CardId, string Data) : IRequestWrapper<string>;

public class ReadTitleQueryHandler : CardRequestHandlerBase<ReadTitleQuery, string>
{
    public ReadTitleQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override Task<ServiceResult<string>> Handle(ReadTitleQuery request, CancellationToken cancellationToken)
    {
      throw new NotImplementedException();  
    }
}
