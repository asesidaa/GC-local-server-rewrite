using MudBlazor;
using WebUI.Services;

namespace WebUI.Common;

public partial class MainLayout : IDisposable
{
    bool drawerOpen = true;

    public bool IsDarkMode { get; set; }

    public MudThemeProvider MudThemeProvider { get; set; } = null!;

    [Inject]
    public required AuthStateService AuthState { get; set; }

    [Inject]
    public required NavigationManager Navigation { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            IsDarkMode = await MudThemeProvider.GetSystemDarkModeAsync();
            await AuthState.CheckAuthAsync();
            StateHasChanged();
        }
    }

    protected override void OnInitialized()
    {
        AuthState.OnAuthStateChanged += StateHasChanged;
    }

    void DrawerToggle()
    {
        drawerOpen = !drawerOpen;
    }

    async Task Logout()
    {
        await AuthState.LogoutAsync();
        Navigation.NavigateTo("/");
    }

    public void Dispose()
    {
        AuthState.OnAuthStateChanged -= StateHasChanged;
    }
}
