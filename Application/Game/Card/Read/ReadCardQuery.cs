using Application.Common.Extensions;
using Application.Common.Models;
using Application.Interfaces;
using Application.Mappers;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Game.Card.Read;

public record ReadCardQuery(long CardId) : IRequestWrapper<string>;

public class ReadCardQueryHandler : CardRequestHandlerBase<ReadCardQuery, string>
{
    private readonly ILogger<ReadCardQueryHandler> logger;

    public ReadCardQueryHandler(ICardDependencyAggregate aggregate, ILogger<ReadCardQueryHandler> logger) : base(aggregate) {
        this.logger = logger;
    }
    
    public override async Task<ServiceResult<string>> Handle(ReadCardQuery request, CancellationToken cancellationToken)
    {
        var card = await CardDbContext.CardMains.FirstOrDefaultAsync(card => card.CardId == request.CardId, cancellationToken: cancellationToken);
        if (card is null)
        {
            logger.LogInformation("Card with {CardId} does not exist! Registering a new one...", request.CardId);
            return ServiceResult.Failed<string>(new ServiceError($"Card id: {request.CardId} does not exist!", (int)CardReturnCode.CardNotRegistered));
        }

        var result = card.CardMainToCardDto().SerializeCardData("/root/card");

        return new ServiceResult<string>(result);
    }
}