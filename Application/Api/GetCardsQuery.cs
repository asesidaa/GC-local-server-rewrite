namespace Application.Api;

public record GetCardsQuery() : IRequestWrapper<List<ClientCardDto>>;

public class GetCardsQueryHandler : RequestHandlerBase<GetCardsQuery, List<ClientCardDto>>
{
    public GetCardsQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override async Task<ServiceResult<List<ClientCardDto>>> Handle(GetCardsQuery request, CancellationToken cancellationToken)
    {
        var cards = await CardDbContext.CardMains.ToListAsync(cancellationToken: cancellationToken);
        var dtoList = cards.Select(card => card.CardMainToClientDto()).ToList();

        return new ServiceResult<List<ClientCardDto>>(dtoList);
    }
}