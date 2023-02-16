using Microsoft.Extensions.Logging;

namespace Application.Api;

public record SetPlayOptionCommand(PlayOptionData Data) : IRequestWrapper<bool>;

public class SetPlayOptionCommandHandler : RequestHandlerBase<SetPlayOptionCommand, bool>
{
    private readonly ILogger<SetPlayOptionCommandHandler> logger;

    public SetPlayOptionCommandHandler(ICardDependencyAggregate aggregate, ILogger<SetPlayOptionCommandHandler> logger) : base(aggregate)
    {
        this.logger = logger;
    }

    public override async Task<ServiceResult<bool>> Handle(SetPlayOptionCommand request, CancellationToken cancellationToken)
    {
        var optionDetail1 = await CardDbContext.CardDetails.FirstOrDefaultAsync(detail =>
            detail.CardId == request.Data.CardId &&
            detail.Pcol1  == 0                   &&
            detail.Pcol2  == 0                   &&
            detail.Pcol3  == 0, 
            cancellationToken: cancellationToken);
        var optionDetail2 = await CardDbContext.CardDetails.FirstOrDefaultAsync(detail =>
            detail.CardId == request.Data.CardId &&
            detail.Pcol1  == 1                   &&
            detail.Pcol2  == 0                   &&
            detail.Pcol3  == 0, 
            cancellationToken: cancellationToken);

        if (optionDetail1 is null ||
            optionDetail2 is null)      
        {
            logger.LogWarning("Attempt to set play options for card id {CardId} failed due to missing data",
                request.Data.CardId);
            return ServiceResult.Failed<bool>(ServiceError.CustomMessage("At least one of the play option records not found"));
        }

        request.Data.OptionPart1.MapFirstOptionDetail(optionDetail1);
        request.Data.OptionPart2.MapSecondOptionDetail(optionDetail2);

        CardDbContext.CardDetails.Update(optionDetail1);
        CardDbContext.CardDetails.Update(optionDetail2);
        
        var count = await CardDbContext.SaveChangesAsync(cancellationToken);
        return count == 1 ? new ServiceResult<bool>(true) : ServiceResult.Failed<bool>(ServiceError.DatabaseSaveFailed);
    }
}