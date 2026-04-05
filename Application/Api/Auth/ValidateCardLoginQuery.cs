using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Api.Auth;

public record ValidateCardLoginQuery(long CardId, string AccessCode) : IRequestWrapper<bool>;

public class ValidateCardLoginQueryHandler : RequestHandlerBase<ValidateCardLoginQuery, bool>
{
    private readonly IPasswordHasher<CardAccessCode> passwordHasher;

    public ValidateCardLoginQueryHandler(ICardDependencyAggregate aggregate,
        IPasswordHasher<CardAccessCode> passwordHasher) : base(aggregate)
    {
        this.passwordHasher = passwordHasher;
    }

    public override async Task<ServiceResult<bool>> Handle(ValidateCardLoginQuery request,
        CancellationToken cancellationToken)
    {
        var accessCode = await CardDbContext.CardAccessCodes
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.CardId == request.CardId, cancellationToken);

        if (accessCode is null)
            return ServiceResult.Failed<bool>(ServiceError.CustomMessage("No access code set for this card"));

        var result = passwordHasher.VerifyHashedPassword(accessCode, accessCode.HashedCode, request.AccessCode);

        return result == PasswordVerificationResult.Failed
            ? ServiceResult.Failed<bool>(ServiceError.CustomMessage("Invalid access code"))
            : new ServiceResult<bool>(true);
    }
}
