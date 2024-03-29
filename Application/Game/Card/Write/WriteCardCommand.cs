﻿using Application.Common.Helpers;
using Microsoft.Extensions.Logging;

namespace Application.Game.Card.Write;

public record WriteCardCommand(long CardId, string Data) : IRequestWrapper<string>;

public class WriteCommandHandler : RequestHandlerBase<WriteCardCommand, string>
{
    private readonly ILogger<WriteCommandHandler> logger;

    public WriteCommandHandler(ICardDependencyAggregate aggregate, ILogger<WriteCommandHandler> logger) : base(aggregate)
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
            card.Modified = TimeHelper.CurrentTimeToString();
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