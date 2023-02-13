using Application.Common.Extensions;
using Application.Common.Models;
using Application.Dto;
using Application.Interfaces;
using Application.Mappers;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Game.Card.Write;

public record WriteCardDetailCommand(long CardId, string Data) : IRequestWrapper<string>;

public class WriteCardDetailCommandHandler : CardRequestHandlerBase<WriteCardDetailCommand, string>
{
    private readonly ILogger<WriteCardDetailCommandHandler> logger;

    public WriteCardDetailCommandHandler(ICardDependencyAggregate aggregate, ILogger<WriteCardDetailCommandHandler> logger) : base(aggregate)
    {
        this.logger = logger;
    }

    public override async Task<ServiceResult<string>> Handle(WriteCardDetailCommand request, CancellationToken cancellationToken)
    {
        var exists = await CardDbContext.CardMains.AnyAsync(card => card.CardId == request.CardId, cancellationToken: cancellationToken);
        if (!exists)
        {
            logger.LogWarning("Card id: {CardId} does not exist!", request.CardId);
            return ServiceResult.Failed<string>(
                new ServiceError($"Card id: {request.CardId} does not exist!", (int)CardReturnCode.CardNotRegistered));
        }

        var dto = request.Data.DeserializeCardData<CardDetailDto>();
        var detail = dto.DtoToCardDetail();
        detail.CardId = request.CardId;
        detail.LastPlayTime = DateTime.Now;
        CardDbContext.CardDetails.Upsert(detail);

        await CardDbContext.SaveChangesAsync(cancellationToken);
        
        return new ServiceResult<string>(request.Data);
    }
}