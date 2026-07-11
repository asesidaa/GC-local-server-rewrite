using Application.Common.Helpers;
using Microsoft.Extensions.Logging;

namespace Application.Api;

public record SetPlayerNameCommand(ClientCardDto Card) : ICommandWrapper<bool>;

public class SetPlayerNameCommandHandler : CommandHandlerBase<SetPlayerNameCommand, bool>
{
    private readonly ILogger<SetPlayerNameCommandHandler> logger;
    public SetPlayerNameCommandHandler(ICardDependencyAggregate aggregate, ILogger<SetPlayerNameCommandHandler> logger) : base(aggregate)
    {
        this.logger = logger;
    }

    public override async ValueTask<ServiceResult<bool>> Handle(SetPlayerNameCommand request, CancellationToken cancellationToken)
    {
        var card = await CardDbContext.CardMains.FirstOrDefaultAsync(card => card.CardId == request.Card.CardId, cancellationToken: cancellationToken);

        if (card is null)
        {
            logger.LogWarning("Attempt to set name for a non existing card {CardId}", request.Card.CardId);
            return ServiceResult.Failed<bool>(ServiceError.UserNotFound);
        }

        card.PlayerName = request.Card.PlayerName;
        card.Modified = TimeHelper.CurrentTimeToString();

        CardDbContext.CardMains.Update(card);
        var count = await CardDbContext.SaveChangesAsync(cancellationToken);
        return count == 1 ? new ServiceResult<bool>(true) : ServiceResult.Failed<bool>(ServiceError.DatabaseSaveFailed);
    }
}