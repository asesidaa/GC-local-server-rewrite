using System.Net.Http.Json;
using Application.Common.Models;
using Application.Dto.Api;
using Microsoft.AspNetCore.Components;

namespace WebUI.Pages;

public partial class Cards
{
    [Inject]
    public HttpClient Client { get; set; } = null!;
    
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
}