using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Shared.Dto.Api;
using Shared.Models;

namespace WebUI.Pages;

public partial class Cards
{
    [Inject]
    public required HttpClient Client { get; set; }

    [Inject]
    public required IDialogService DialogService { get; set; }
    
    private List<ClientCardDto>? CardDtos { get; set; }

    private string ErrorMessage { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        var result = await Client.GetFromJsonAsync<ServiceResult<List<ClientCardDto>>>("api/Profiles");
        if (result is null)
        {
            ErrorMessage = "Parse result failed";
            return;
        }

        if (!result.Succeeded)
        {
            ErrorMessage = result.Error!.Message;
            return;
        }
        CardDtos = result.Data;
    }

    private async Task OnEditPlayerNameClicked(ClientCardDto card)
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = false,
            DisableBackdropClick = true,
            FullWidth = true
        };
        var parameters = new DialogParameters { { "Data", card } };
        var dialog = await DialogService.ShowAsync<ChangePlayerNameDialog>("Favorite", parameters, options);
        var result = await dialog.Result;
    }
}