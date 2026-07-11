namespace Application.Api;

public record GetCardsQuery() : IQueryWrapper<List<ClientCardDto>>;

public class GetCardsQueryHandler : QueryHandlerBase<GetCardsQuery, List<ClientCardDto>>
{
    public GetCardsQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override async ValueTask<ServiceResult<List<ClientCardDto>>> Handle(GetCardsQuery request, CancellationToken cancellationToken)
    {
        var cards = await CardDbContext.CardMains.ToListAsync(cancellationToken: cancellationToken);
        var dtoList = cards.Select(card => card.CardMainToClientDto()).ToList();

        return new ServiceResult<List<ClientCardDto>>(dtoList);
    }
}