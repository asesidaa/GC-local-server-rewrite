using Domain.Config;
using Microsoft.Extensions.Options;

namespace Application.Api.Auth;

public record ValidateAdminLoginQuery(string Password) : IRequestWrapper<bool>;

public class ValidateAdminLoginQueryHandler : IRequestHandlerWrapper<ValidateAdminLoginQuery, bool>
{
    private readonly WebAuthConfig config;

    public ValidateAdminLoginQueryHandler(IOptions<WebAuthConfig> options)
    {
        config = options.Value;
    }

    public Task<ServiceResult<bool>> Handle(ValidateAdminLoginQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(config.AdminPassword))
            return Task.FromResult(ServiceResult.Failed<bool>(ServiceError.CustomMessage("Admin login is not configured")));

        var valid = request.Password == config.AdminPassword;
        return valid
            ? Task.FromResult(new ServiceResult<bool>(true))
            : Task.FromResult(ServiceResult.Failed<bool>(ServiceError.CustomMessage("Invalid password")));
    }
}
