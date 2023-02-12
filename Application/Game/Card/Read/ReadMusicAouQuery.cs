using Application.Common.Extensions;
using Application.Common.Models;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Game.Card.Read;


public record ReadMusicAouQuery(long CardId, string Data) : IRequestWrapper<string>;

public class ReadMusicAouQueryHandler : CardRequestHandlerBase<ReadMusicAouQuery, string>
{
    public ReadMusicAouQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override Task<ServiceResult<string>> Handle(ReadMusicAouQuery request, CancellationToken cancellationToken)
    {
      throw new NotImplementedException();  
    }
}
