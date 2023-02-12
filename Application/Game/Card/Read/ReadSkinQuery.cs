using Application.Common.Extensions;
using Application.Common.Models;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Game.Card.Read;


public record ReadSkinQuery(long CardId) : IRequestWrapper<string>;

public class ReadSkinQueryHandler : CardRequestHandlerBase<ReadSkinQuery, string>
{
    public ReadSkinQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override Task<ServiceResult<string>> Handle(ReadSkinQuery request, CancellationToken cancellationToken)
    {
      throw new NotImplementedException();  
    }
}
