using Shared.Models;

namespace Application.Api;

public record GetPlayOptionQuery(long CardId) : IRequestWrapper<PlayOptionData>;

public class GetPlayOptionQueryHandler : RequestHandlerBase<GetPlayOptionQuery, PlayOptionData>
{
    public GetPlayOptionQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override async Task<ServiceResult<PlayOptionData>> Handle(GetPlayOptionQuery request,
        CancellationToken                                                               cancellationToken)
    {
        var optionDetail1 = await CardDbContext.CardDetails.FirstOrDefaultAsync(detail =>
                detail.CardId == request.CardId &&
                detail.Pcol1  == 0              &&
                detail.Pcol2  == 0              &&
                detail.Pcol3  == 0,
            cancellationToken: cancellationToken);
        var optionDetail2 = await CardDbContext.CardDetails.FirstOrDefaultAsync(detail =>
                detail.CardId == request.CardId &&
                detail.Pcol1  == 1              &&
                detail.Pcol2  == 0              &&
                detail.Pcol3  == 0,
            cancellationToken: cancellationToken);
        if (optionDetail1 is null ||
            optionDetail2 is null)      
        {
            return ServiceResult.Failed<PlayOptionData>(ServiceError.CustomMessage("At least one of the play option records not found"));
        }

        var result = new PlayOptionData
        {
            CardId = request.CardId,
            OptionPart1 = optionDetail1.CardDetailToFirstOption(),
            OptionPart2 = optionDetail2.CardDetailToSecondOption()
        };

        return new ServiceResult<PlayOptionData>(result);
    }
}