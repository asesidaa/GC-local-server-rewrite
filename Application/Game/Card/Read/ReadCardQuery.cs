using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Application.Game.Card.Read;

public record ReadCardQuery(long CardId) : IRequestWrapper<string>;

public class ReadQueryHandler : RequestHandlerBase<ReadCardQuery, string>
{
    private readonly ILogger<ReadQueryHandler> logger;

    public ReadQueryHandler(ICardDependencyAggregate aggregate, ILogger<ReadQueryHandler> logger) : base(aggregate) {
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