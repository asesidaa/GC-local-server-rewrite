using System.Security.Claims;
using Application.Api.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Dto.Api;
using Shared.Models;

namespace MainServer.Controllers.API;

[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseController<AuthController>
{
    [HttpPost("LoginAdmin")]
    public async Task<ServiceResult<AuthStatusDto>> LoginAdmin(AdminLoginRequest request)
    {
        var result = await Mediator.Send(new ValidateAdminLoginQuery(request.Password));
        if (!result.Succeeded)
            return ServiceResult.Failed<AuthStatusDto>(result.Error!);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Role, "Admin"),
            new(ClaimTypes.Name, "Admin")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return new ServiceResult<AuthStatusDto>(new AuthStatusDto
        {
            IsAuthenticated = true,
            Role = "Admin"
        });
    }

    [HttpPost("LoginCard")]
    public async Task<ServiceResult<AuthStatusDto>> LoginCard(CardLoginRequest request)
    {
        var result = await Mediator.Send(new ValidateCardLoginQuery(request.CardId, request.AccessCode));
        if (!result.Succeeded)
            return ServiceResult.Failed<AuthStatusDto>(result.Error!);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Role, "Player"),
            new(ClaimTypes.Name, request.CardId.ToString()),
            new("CardId", request.CardId.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return new ServiceResult<AuthStatusDto>(new AuthStatusDto
        {
            IsAuthenticated = true,
            Role = "Player",
            CardId = request.CardId
        });
    }

    [HttpPost("Logout")]
    public async Task<ServiceResult<bool>> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return new ServiceResult<bool>(true);
    }

    [HttpGet("Me")]
    public ServiceResult<AuthStatusDto> GetAuthStatus()
    {
        var user = HttpContext.User;
        if (user.Identity?.IsAuthenticated != true)
        {
            return new ServiceResult<AuthStatusDto>(new AuthStatusDto
            {
                IsAuthenticated = false
            });
        }

        var role = user.FindFirstValue(ClaimTypes.Role);
        var cardIdStr = user.FindFirstValue("CardId");
        long? cardId = long.TryParse(cardIdStr, out var parsed) ? parsed : null;

        return new ServiceResult<AuthStatusDto>(new AuthStatusDto
        {
            IsAuthenticated = true,
            Role = role,
            CardId = cardId
        });
    }

    [Authorize]
    [HttpPost("SetAccessCode")]
    public async Task<ServiceResult<bool>> SetAccessCode(SetAccessCodeRequest request)
    {
        // Admin can set for any card; Player can only set for their own card
        var role = HttpContext.User.FindFirstValue(ClaimTypes.Role);
        if (role != "Admin")
        {
            var cardIdStr = HttpContext.User.FindFirstValue("CardId");
            if (!long.TryParse(cardIdStr, out var ownCardId) || ownCardId != request.CardId)
                return ServiceResult.Failed<bool>(ServiceError.CustomMessage("You can only change your own access code"));
        }

        return await Mediator.Send(new SetCardAccessCodeCommand(request.CardId, request.AccessCode));
    }
}
