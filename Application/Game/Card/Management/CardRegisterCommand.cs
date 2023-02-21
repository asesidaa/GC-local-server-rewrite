using Application.Common.Helpers;
using Microsoft.Extensions.Logging;

namespace Application.Game.Card.Management;

public record CardRegisterCommand(long CardId, string Data) : IRequestWrapper<string>;

public class RegisterCommandHandler : RequestHandlerBase<CardRegisterCommand, string>
{
    private readonly ILogger<RegisterCommandHandler> logger;
    public RegisterCommandHandler(ICardDependencyAggregate aggregate, ILogger<RegisterCommandHandler> logger) : base(aggregate)
    {
        this.logger = logger;
    }

    public override async Task<ServiceResult<string>> Handle(CardRegisterCommand request, CancellationToken cancellationToken)
    {
        var exists = CardDbContext.CardMains.Any(card => card.CardId == request.CardId);
        if (exists)
        {
            return ServiceResult.Failed<string>(ServiceError.CustomMessage($"Card {request.CardId} already exists!"));
        }
        
        var card = request.Data.DeserializeCardData<CardDto>().CardDtoToCardMain();
        card.CardId = request.CardId;
        card.Created = TimeHelper.CurrentTimeToString();
        card.Modified = card.Created;
        logger.LogInformation("New card {{Id: {Id}, Player Name: {Name}}} registered", card.CardId, card.PlayerName);
        CardDbContext.CardMains.Add(card);
        await CardDbContext.SaveChangesAsync(cancellationToken);
        
        return new ServiceResult<string>(request.Data);
    }
}