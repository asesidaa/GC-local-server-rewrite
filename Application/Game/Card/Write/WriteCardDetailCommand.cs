using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Application.Game.Card.Write;

public record WriteCardDetailCommand(long CardId, string Data) : IRequestWrapper<string>;

public class WriteDetailCommandHandler : RequestHandlerBase<WriteCardDetailCommand, string>
{
    private readonly ILogger<WriteDetailCommandHandler> logger;

    public WriteDetailCommandHandler(ICardDependencyAggregate aggregate, ILogger<WriteDetailCommandHandler> logger) : base(aggregate)
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
        logger.LogInformation("RequestData:"+request.Data);
        var dto = request.Data.DeserializeCardData<CardDetailDto>();
        var info = request.Data.DeserializeCardInfo<CardDetailInfoDto>();
        var detail = dto.DtoToCardDetail();
        detail.CardId = request.CardId;
        detail.LastPlayTime = DateTime.Now;
        detail.LastPlayTenpoId = info.TenpoId;
        logger.LogInformation($"CardID:{detail.CardId}/LastPlayTenpoID:{detail.LastPlayTenpoId}/TenpoIDInfo:{info.TenpoId}");
        await CardDbContext.CardDetails.Upsert(detail).RunAsync(cancellationToken);

        await CardDbContext.SaveChangesAsync(cancellationToken);
        
        return new ServiceResult<string>(request.Data);
    }
}