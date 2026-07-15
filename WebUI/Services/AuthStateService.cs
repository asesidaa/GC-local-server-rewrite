using Shared.Dto.Api;

namespace WebUI.Services;

public class AuthStateService
{
    private readonly HttpClient client;

    public AuthStateService(HttpClient client)
    {
        this.client = client;
    }

    public bool IsAuthenticated { get; private set; }
    public bool IsAdmin { get; private set; }
    public bool IsPlayer { get; private set; }
    public long? CardId { get; private set; }

    public event Action? OnAuthStateChanged;

    public async Task CheckAuthAsync()
    {
        try
        {
            var result = await client.GetFromJsonAsync<ServiceResult<AuthStatusDto>>("api/Auth/Me");
            if (result is { Succeeded: true, Data: not null })
            {
                ApplyStatus(result.Data);
            }
            else
            {
                ClearState();
            }
        }
        catch
        {
            ClearState();
        }
    }

    public async Task<ServiceResult<AuthStatusDto>?> LoginAdminAsync(string password)
    {
        var response = await client.PostAsJsonAsync("api/Auth/LoginAdmin", new AdminLoginRequest { Password = password });
        var result = await response.Content.ReadFromJsonAsync<ServiceResult<AuthStatusDto>>();
        if (result is { Succeeded: true, Data: not null })
        {
            ApplyStatus(result.Data);
        }
        return result;
    }

    public async Task<ServiceResult<AuthStatusDto>?> LoginCardAsync(long cardId, string accessCode)
    {
        var response = await client.PostAsJsonAsync("api/Auth/LoginCard",
            new CardLoginRequest { CardId = cardId, AccessCode = accessCode });
        var result = await response.Content.ReadFromJsonAsync<ServiceResult<AuthStatusDto>>();
        if (result is { Succeeded: true, Data: not null })
        {
            ApplyStatus(result.Data);
        }
        return result;
    }

    public async Task LogoutAsync()
    {
        await client.PostAsync("api/Auth/Logout", null);
        ClearState();
    }

    private void ApplyStatus(AuthStatusDto status)
    {
        IsAuthenticated = status.IsAuthenticated;
        IsAdmin = status.Role == "Admin";
        IsPlayer = status.Role == "Player";
        CardId = status.CardId;
        OnAuthStateChanged?.Invoke();
    }

    private void ClearState()
    {
        IsAuthenticated = false;
        IsAdmin = false;
        IsPlayer = false;
        CardId = null;
        OnAuthStateChanged?.Invoke();
    }
}
