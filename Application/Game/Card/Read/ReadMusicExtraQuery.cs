using Application.Common.Extensions;
using Application.Common.Models;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Game.Card.Read;


public record ReadMusicExtraQuery(long CardId, string Data) : IRequestWrapper<string>;

public class ReadMusicExtraQueryHandler : CardRequestHandlerBase<ReadMusicExtraQuery, string>
{
    public ReadMusicExtraQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override Task<ServiceResult<string>> Handle(ReadMusicExtraQuery request, CancellationToken cancellationToken)
    {
      throw new NotImplementedException();  
    }
}
