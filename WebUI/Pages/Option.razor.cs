using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Shared.Models;
using Throw;
using WebUI.Services;
using SourceGenerationContext = Shared.SerializerContexts.SourceGenerationContext;

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
    
    private readonly List<BreadcrumbItem> breadcrumbs = new()
    {
        new BreadcrumbItem("Cards", href: "/Cards"),
    };

    private PlayOptionData? playOptionData;

    private string errorMessage = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        breadcrumbs.Add(new BreadcrumbItem($"Card: {CardId}", href:null, disabled:true));
        breadcrumbs.Add(new BreadcrumbItem("Option", href: $"/Cards/Option/{CardId}", disabled: false));

        await Task.Delay(3000);
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
        var navigator = DataService.GetNavigators().GetValueOrDefault(navigatorId);

        return navigator?.NavigatorName ?? "Navigator id unknown";
    }
    
    private string GetAvatarName(uint avatarId)
    {
        var avatar = DataService.GetAvatars().GetValueOrDefault(avatarId);

        return avatar?.AvatarName ?? "Avatar id unknown";
    }
    
    private string GetTitleName(uint titleId)
    {
        var title = DataService.GetTitles().GetValueOrDefault(titleId);

        return title?.TitleName ?? "Title id unknown";
    }
}