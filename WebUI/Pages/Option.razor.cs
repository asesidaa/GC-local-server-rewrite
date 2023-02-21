using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Shared.Models;
using Throw;
using WebUI.Pages.Dialogs;
using WebUI.Services;

namespace WebUI.Pages;

public partial class Option
{
    [Parameter]
    public long CardId { get; set; }
    
    [Inject]
    public required HttpClient Client { get; set; }

    [Inject]
    public required IDialogService DialogService { get; set; }
    
    [Inject]
    public required IDataService DataService { get; set; }

    private bool isSaving;
    
    private readonly List<BreadcrumbItem> breadcrumbs = new()
    {
        new BreadcrumbItem("Cards", href: "/Cards"),
    };

    private PlayOptionData? playOptionData;

    private string? errorMessage;

    private static readonly DialogOptions OPTIONS = new()
    {
        CloseOnEscapeKey = false,
        DisableBackdropClick = true,
        FullWidth = true,
        MaxWidth = MaxWidth.ExtraExtraLarge
    };

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        breadcrumbs.Add(new BreadcrumbItem($"Card: {CardId}", href:null, disabled:true));
        breadcrumbs.Add(new BreadcrumbItem("Option", href: $"/Cards/Option/{CardId}", disabled: false));

        var result = await Client.GetFromJsonAsync<ServiceResult<PlayOptionData>>($"api/PlayOption/{CardId}");
        result.ThrowIfNull();
        
        if (!result.Succeeded)
        {
            errorMessage = result.Error!.Message;
            return;
        }

        playOptionData = result.Data;
    }

    private string GetNavigatorName(uint navigatorId)
    {
        var navigator = DataService.GetNavigatorById(navigatorId);

        return navigator?.NavigatorName ?? "Navigator id unknown";
    }
    
    private string GetAvatarName(uint avatarId)
    {
        var avatar = DataService.GetAvatarById(avatarId);

        return avatar?.AvatarName ?? "Avatar id unknown";
    }
    
    private string GetTitleName(uint titleId)
    {
        var title = DataService.GetTitleById(titleId);

        return title?.TitleName ?? "Title id unknown";
    }

    private async Task OpenChangeTitleDialog()
    {
        var parameters = new DialogParameters
        {
            ["Data"] = playOptionData
        };

        var dialog = await DialogService.ShowAsync<ChangeTitleDialog>("Change Title", parameters, OPTIONS);
        var result = await dialog.Result;
        if (!result.Canceled)
        {
            StateHasChanged();
        }
    }
    
    private async Task OpenChangeNavigatorDialog()
    {
        var parameters = new DialogParameters
        {
            ["Data"] = playOptionData
        };

        var dialog = await DialogService.ShowAsync<ChangeNavigatorDialog>("Change Navigator", parameters, OPTIONS);
        var result = await dialog.Result;
        if (!result.Canceled)
        {
            StateHasChanged();
        }
    }
    private async Task OpenChangeAvatarDialog()
    {
        var parameters = new DialogParameters
        {
            ["Data"] = playOptionData
        };

        var dialog = await DialogService.ShowAsync<ChangeAvatarDialog>("Change Navigator", parameters, OPTIONS);
        var result = await dialog.Result;
        if (!result.Canceled)
        {
            StateHasChanged();
        }
    }

    private async Task SaveOptions()
    {
        isSaving = true;
        var result = await Client.PostAsJsonAsync("api/PlayOption", playOptionData);
        isSaving = false;
    }

    private async Task UnlockMusics()
    {
        await Client.PostAsync($"api/Profiles/UnlockAllMusic/{CardId}", null);
    }
}