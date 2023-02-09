using Application.Common.Extensions;
using Application.Common.Models;
using Application.Dto;
using Application.Interfaces;
using Application.Mappers;

namespace Application.Game.Card.Management;

public record CardRegisterCommand(long CardId, string Data) : IRequestWrapper<string>;

public class CardRegisterCommandHandler : CardRequestHandlerBase<CardRegisterCommand, string>
{
    public CardRegisterCommandHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override async Task<ServiceResult<string>> Handle(CardRegisterCommand request, CancellationToken cancellationToken)
    {
        var exists = CardDbContext.CardMains.Any(card => card.CardId == request.CardId);
        if (!exists)
        {
            return ServiceResult.Failed<string>(ServiceError.CustomMessage($"Card {request.CardId} already exists!"));
        }
        
        var card = request.Data.DeserializeCardData<CardDto>().CardDtoToCardMain();
        card.CardId = request.CardId;
        CardDbContext.CardMains.Add(card);
        await CardDbContext.SaveChangesAsync(cancellationToken);
        
        return new ServiceResult<string>(request.Data);
    }
}