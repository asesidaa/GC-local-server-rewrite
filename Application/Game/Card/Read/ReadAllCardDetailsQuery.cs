using System.Diagnostics.CodeAnalysis;
using Application.Common.Extensions;
using Application.Common.Models;
using Application.Interfaces;
using Application.Mappers;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Game.Card.Read;


public record ReadAllCardDetailsQuery(long CardId) : IRequestWrapper<string>;

public class ReadAllCardDetailsQueryHandler : CardRequestHandlerBase<ReadAllCardDetailsQuery, string>
{
    private const string CARD_DETAILS_XPATH = "/root/card_detail";
    private const string RECORD_XPATH = $"{CARD_DETAILS_XPATH}/record";
    
    private readonly ILogger<ReadAllCardDetailsQueryHandler> logger;

    public ReadAllCardDetailsQueryHandler(ICardDependencyAggregate aggregate, ILogger<ReadAllCardDetailsQueryHandler> logger) : base(aggregate)
    {
        this.logger = logger;
    }

    [SuppressMessage("ReSharper.DPA", "DPA0007: Large number of DB records", 
        Justification = "Card details will return all records by design, which results in a large number of DB records")]
    public override async Task<ServiceResult<string>> Handle(ReadAllCardDetailsQuery request, CancellationToken cancellationToken)
    {
        var exists = await CardDbContext.CardMains.AnyAsync(card => card.CardId == request.CardId, cancellationToken: cancellationToken);
        if (!exists)
        {
            logger.LogWarning("Card id: {CardId} does not exist!", request.CardId);
            return ServiceResult.Failed<string>(
                new ServiceError($"Card id: {request.CardId} does not exist!", (int)CardReturnCode.CardNotRegistered));
        }

        var cardDetails = await CardDbContext.CardDetails
            .Where(detail => detail.CardId == request.CardId)
            .ToListAsync(cancellationToken: cancellationToken);

        string result;
        if (cardDetails.Count == 0)
        {
            result = new object().SerializeCardData(CARD_DETAILS_XPATH);
        }
        else
        {
            var dtoList = cardDetails.Select((detail, i) =>
            {
                var dto = detail.CardDetailToDto();
                dto.Id = i;
                return dto;
            });
            result = dtoList.SerializeCardDataList(RECORD_XPATH);
        }

        return new ServiceResult<string>(result);
    }
}
