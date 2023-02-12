using Application.Common.Extensions;
using Application.Common.Models;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Game.Card.Read;


public record ReadMusicQuery(long CardId) : IRequestWrapper<string>;

public class ReadMusicQueryHandler : CardRequestHandlerBase<ReadMusicQuery, string>
{
    public ReadMusicQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override Task<ServiceResult<string>> Handle(ReadMusicQuery request, CancellationToken cancellationToken)
    {
      throw new NotImplementedException();  
    }
}
