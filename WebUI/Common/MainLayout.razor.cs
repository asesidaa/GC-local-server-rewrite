using MudBlazor;

namespace WebUI.Common;

public partial class MainLayout
{
    bool drawerOpen = true;
    
    public bool IsDarkMode { get; set; }

    public MudThemeProvider MudThemeProvider { get; set; } = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            IsDarkMode = await MudThemeProvider.GetSystemDarkModeAsync();
            StateHasChanged();
        }
    }

    void DrawerToggle()
    {
        drawerOpen = !drawerOpen;
    }
}