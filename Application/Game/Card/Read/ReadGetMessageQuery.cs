using Application.Common.Extensions;
using Application.Common.Models;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Game.Card.Read;


public record ReadGetMessageQuery(long CardId) : IRequestWrapper<string>;

public class ReadGetMessageQueryHandler : CardRequestHandlerBase<ReadGetMessageQuery, string>
{
    public ReadGetMessageQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override Task<ServiceResult<string>> Handle(ReadGetMessageQuery request, CancellationToken cancellationToken)
    {
      throw new NotImplementedException();  
    }
}
