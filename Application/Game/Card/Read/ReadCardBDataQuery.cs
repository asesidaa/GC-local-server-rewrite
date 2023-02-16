using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Application.Game.Card.Read;


public record ReadCardBDataQuery(long CardId) : IRequestWrapper<string>;

public class ReadBDataQueryHandler : RequestHandlerBase<ReadCardBDataQuery, string>
{
    private const string CARD_BDATA_XPATH = "/root/card_bdata";
    
    private readonly ILogger<ReadBDataQueryHandler> logger;

    public ReadBDataQueryHandler(ICardDependencyAggregate aggregate, ILogger<ReadBDataQueryHandler> logger) : base(aggregate)
    {
        this.logger = logger;
    }

    public override async Task<ServiceResult<string>> Handle(ReadCardBDataQuery request, CancellationToken cancellationToken)
    {
        var exists = await CardDbContext.CardMains.AnyAsync(card => card.CardId == request.CardId, cancellationToken: cancellationToken);
        if (!exists)
        {
            logger.LogWarning("Card id: {CardId} does not exist!", request.CardId);
            return ServiceResult.Failed<string>(
                new ServiceError($"Card id: {request.CardId} does not exist!", (int)CardReturnCode.CardNotRegistered));
        }

        var bdata = await CardDbContext.CardBdata.FirstOrDefaultAsync(
            card => card.CardId == request.CardId, cancellationToken: cancellationToken);

        var result = bdata?.CardBDatumToDto().SerializeCardData(CARD_BDATA_XPATH)
                     ?? new object().SerializeCardData(CARD_BDATA_XPATH);

        return new ServiceResult<string>(result);
    }
}
