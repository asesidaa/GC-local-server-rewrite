using Microsoft.Extensions.Logging;

namespace Application.Api;

public record SetFavoriteMusicCommand(MusicFavoriteDto Data) : IRequestWrapper<bool>;

public class SetFavoriteMusicCommandHandler : RequestHandlerBase<SetFavoriteMusicCommand, bool>
{
    private readonly ILogger<SetFavoriteMusicCommandHandler> logger;

    public SetFavoriteMusicCommandHandler(ICardDependencyAggregate aggregate, ILogger<SetFavoriteMusicCommandHandler> logger) : base(aggregate)
    {
        this.logger = logger;
    }

    public override async Task<ServiceResult<bool>> Handle(SetFavoriteMusicCommand request,
        CancellationToken                                                          cancellationToken)
    {
        var musicDetail = await CardDbContext.CardDetails.FirstOrDefaultAsync(detail =>
                detail.CardId == request.Data.CardId  &&
                detail.Pcol1  == 10                   &&
                detail.Pcol2  == request.Data.MusicId &&
                detail.Pcol3  == 0,
            cancellationToken);

        if (musicDetail is null)
        {
            logger.LogWarning("Attempt to set favorite for non existing music, card id: {CardId}, music id: {MusicId}",
                request.Data.CardId, request.Data.MusicId);
            return ServiceResult.Failed<bool>(ServiceError.CustomMessage("Music record not found"));
        }

        musicDetail.Fcol1 = request.Data.IsFavorite ? 1 : 0;
        CardDbContext.CardDetails.Update(musicDetail);
        var count = await CardDbContext.SaveChangesAsync(cancellationToken);

        return count == 1 ? new ServiceResult<bool>(true) : ServiceResult.Failed<bool>(ServiceError.DatabaseSaveFailed);
    }
}