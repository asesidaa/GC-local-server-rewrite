using Application.Common.Extensions;
using Application.Common.Helpers;
using Application.Common.Models;
using Application.Dto;
using Application.Interfaces;
using Application.Mappers;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Game.Card.Write;

public record WriteCardCommand(long CardId, string Data) : IRequestWrapper<string>;

public class WriteCardCommandHandler : CardRequestHandlerBase<WriteCardCommand, string>
{
    private readonly ILogger<WriteCardCommandHandler> logger;

    public WriteCardCommandHandler(ICardDependencyAggregate aggregate, ILogger<WriteCardCommandHandler> logger) : base(aggregate)
    {
        this.logger = logger;
    }

    public override async Task<ServiceResult<string>> Handle(WriteCardCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Data.DeserializeCardData<CardDto>();
        dto.CardId = request.CardId;

        var card = await CardDbContext.CardMains.FirstOrDefaultAsync(card => card.CardId == request.CardId, cancellationToken: cancellationToken);

        if (card is null)
        {
            logger.LogInformation("Creating new card {CardId}", request.CardId);
            card = dto.CardDtoToCardMain();
            card.Created = TimeHelper.CurrentTimeToString();
            CardDbContext.CardMains.Add(card);
        }
        else
        {
            logger.LogInformation("Updating {CardId}", request.CardId);
            card.Fcol1 = dto.Fcol1;
            card.Fcol2 = dto.Fcol2;
            card.Fcol3 = dto.Fcol3;
            card.ScoreI1 = dto.ScoreI1;
            card.Modified = TimeHelper.CurrentTimeToString();
            CardDbContext.CardMains.Update(card);
        }

        await CardDbContext.SaveChangesAsync(cancellationToken);
        
        return new ServiceResult<string>(request.Data);
    }
}