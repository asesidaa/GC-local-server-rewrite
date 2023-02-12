using Application.Common.Extensions;
using Application.Common.Models;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Game.Card.Read;


public record ReadSoundEffectQuery(long CardId, string Data) : IRequestWrapper<string>;

public class ReadSoundEffectQueryHandler : CardRequestHandlerBase<ReadSoundEffectQuery, string>
{
    public ReadSoundEffectQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override Task<ServiceResult<string>> Handle(ReadSoundEffectQuery request, CancellationToken cancellationToken)
    {
      throw new NotImplementedException();  
    }
}
