using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Api.Auth;

public record SetCardAccessCodeCommand(long CardId, string AccessCode) : IRequestWrapper<bool>;

public class SetCardAccessCodeCommandHandler : RequestHandlerBase<SetCardAccessCodeCommand, bool>
{
    private readonly IPasswordHasher<CardAccessCode> passwordHasher;

    public SetCardAccessCodeCommandHandler(ICardDependencyAggregate aggregate,
        IPasswordHasher<CardAccessCode> passwordHasher) : base(aggregate)
    {
        this.passwordHasher = passwordHasher;
    }

    public override async Task<ServiceResult<bool>> Handle(SetCardAccessCodeCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.AccessCode))
            return ServiceResult.Failed<bool>(ServiceError.CustomMessage("Access code cannot be empty"));

        var cardExists = await CardDbContext.CardMains
            .AnyAsync(c => c.CardId == request.CardId, cancellationToken);

        if (!cardExists)
            return ServiceResult.Failed<bool>(ServiceError.CustomMessage("Card not found"));

        var existing = await CardDbContext.CardAccessCodes
            .FirstOrDefaultAsync(a => a.CardId == request.CardId, cancellationToken);

        var dummy = new CardAccessCode { CardId = request.CardId };
        var hashedCode = passwordHasher.HashPassword(dummy, request.AccessCode);

        if (existing is not null)
        {
            existing.HashedCode = hashedCode;
        }
        else
        {
            CardDbContext.CardAccessCodes.Add(new CardAccessCode
            {
                CardId = request.CardId,
                HashedCode = hashedCode
            });
        }

        await CardDbContext.SaveChangesAsync(cancellationToken);

        return new ServiceResult<bool>(true);
    }
}
