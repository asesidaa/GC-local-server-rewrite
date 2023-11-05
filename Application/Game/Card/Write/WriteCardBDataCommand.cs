using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Application.Game.Card.Write;

public record WriteCardBDataCommand(long CardId, string Data) : IRequestWrapper<string>;

public class WriteBDataCommandHandler : RequestHandlerBase<WriteCardBDataCommand, string>
{
    private readonly ILogger<WriteBDataCommandHandler> logger;

    public WriteBDataCommandHandler(ICardDependencyAggregate aggregate, ILogger<WriteBDataCommandHandler> logger) : base(aggregate)
    {
        this.logger = logger;
    }

    public override async Task<ServiceResult<string>> Handle(WriteCardBDataCommand request, CancellationToken cancellationToken)
    {
        var exists = await CardDbContext.CardMains.AnyAsync(card => card.CardId == request.CardId, cancellationToken: cancellationToken);
        if (!exists)
        {
            logger.LogWarning("Card id: {CardId} does not exist!", request.CardId);
            return ServiceResult.Failed<string>(
                new ServiceError($"Card id: {request.CardId} does not exist!", (int)CardReturnCode.CardNotRegistered));
        }

        var dto = request.Data.DeserializeCardData<CardBDatumDto>();
        var data = dto.DtoToCardBDatum();
        data.CardId = request.CardId;
        await CardDbContext.CardBdata.Upsert(data).RunAsync(cancellationToken);

        var cardPlayCount = await CardDbContext.CardPlayCounts
            .FirstOrDefaultAsync(count => count.CardId == request.CardId, cancellationToken);
        if (cardPlayCount is null)
        {
            cardPlayCount = new CardPlayCount
            {
                CardId = request.CardId,
                PlayCount = 0,
                LastPlayedTime = DateTime.Now
            };
        }
        cardPlayCount.PlayCount++;
        cardPlayCount.LastPlayedTime = DateTime.Now;
        await CardDbContext.CardPlayCounts.Upsert(cardPlayCount).RunAsync(cancellationToken);

        await CardDbContext.SaveChangesAsync(cancellationToken);
        
        return new ServiceResult<string>(request.Data);
    }
}