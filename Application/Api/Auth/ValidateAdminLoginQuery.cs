using Domain.Config;
using Microsoft.Extensions.Options;

namespace Application.Api.Auth;

public record ValidateAdminLoginQuery(string Password) : IQueryWrapper<bool>;

public class ValidateAdminLoginQueryHandler : IQueryHandlerWrapper<ValidateAdminLoginQuery, bool>
{
    private readonly WebAuthConfig config;

    public ValidateAdminLoginQueryHandler(IOptions<WebAuthConfig> options)
    {
        config = options.Value;
    }

    public ValueTask<ServiceResult<bool>> Handle(ValidateAdminLoginQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(config.AdminPassword))
            return ValueTask.FromResult(ServiceResult.Failed<bool>(ServiceError.CustomMessage("Admin login is not configured")));

        var valid = request.Password == config.AdminPassword;
        return valid
            ? ValueTask.FromResult(new ServiceResult<bool>(true))
            : ValueTask.FromResult(ServiceResult.Failed<bool>(ServiceError.CustomMessage("Invalid password")));
    }
}
