using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Application.Game.Card.Read;


public record ReadCardDetailQuery(long CardId, string Data) : IRequestWrapper<string>;

public class ReadDetailQueryHandler : RequestHandlerBase<ReadCardDetailQuery, string>
{
    private const string CARD_DETAILS_XPATH = "/root/card_detail";

    private readonly ILogger<ReadDetailQueryHandler> logger;

    public ReadDetailQueryHandler(ICardDependencyAggregate aggregate, ILogger<ReadDetailQueryHandler> logger) : base(aggregate)
    {
        this.logger = logger;
    }

    public override async Task<ServiceResult<string>> Handle(ReadCardDetailQuery request, CancellationToken cancellationToken)
    {
        var exists = await CardDbContext.CardMains.AnyAsync(card => card.CardId == request.CardId, cancellationToken: cancellationToken);
        if (!exists)
        {
            logger.LogWarning("Card id: {CardId} does not exist!", request.CardId);
            return ServiceResult.Failed<string>(
                new ServiceError($"Card id: {request.CardId} does not exist!", (int)CardReturnCode.CardNotRegistered));
        }

        var queryCondition = request.Data.DeserializeCardData<CardDetailDto>();
        var detail = await CardDbContext.CardDetails.FirstOrDefaultAsync(cardDetail =>
            cardDetail.CardId == request.CardId &&
            cardDetail.Pcol1 == queryCondition.Pcol1 &&
            cardDetail.Pcol2 == queryCondition.Pcol2 &&
            cardDetail.Pcol3 == queryCondition.Pcol3, cancellationToken: cancellationToken);

        var dto = detail?.CardDetailToDto();

        var result = dto?.SerializeCardData(CARD_DETAILS_XPATH) ??
                     new object().SerializeCardData(CARD_DETAILS_XPATH);

        return new ServiceResult<string>(result);
    }
}
