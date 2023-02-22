using Throw;
using WebUI.Pages.Dialogs;

namespace WebUI.Pages;

public partial class PlayRecords
{
    private readonly List<BreadcrumbItem> breadcrumbs = new()
    {
        new BreadcrumbItem("Cards", href: "/Cards")
    };
    
    [Parameter]
    public long CardId { get; set; }
    
    [Inject]
    public required HttpClient Client { get; set; }
    
    [Inject]
    public required IDialogService DialogService { get; set; }
    
    private string? errorMessage;

    private List<SongPlayRecord>? songPlayRecords;
    
    protected async override Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        
        breadcrumbs.Add(new BreadcrumbItem($"Card: {CardId}", href:null, disabled:true));
        breadcrumbs.Add(new BreadcrumbItem("TotalResult", href: $"/Cards/PlayRecords/{CardId}", disabled: false));

        var result = await Client.GetFromJsonAsync<ServiceResult<List<SongPlayRecord>>>($"api/Profiles/SongPlayRecords/{CardId}");
        result.ThrowIfNull();
        
        if (!result.Succeeded)
        {
            errorMessage = result.Error!.Message;
            return;
        }

        songPlayRecords = result.Data;
    }

    private static string GetRating(int score) => score switch
    {
        > 990000 => "S++",
        > 950000 => "S+",
        > 900000 => "S",
        > 800000 => "A",
        > 700000 => "B",
        > 500000 => "C",
        > 300000 => "D",
        _        => "E"
    };

    private async Task OnFavoriteToggled(SongPlayRecord data)
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = false,
            DisableBackdropClick = true,
            FullWidth = true
        };

        var parameters = new DialogParameters
        {
            { "Data", data },
            {"CardId", CardId}
        };
        var dialog = await DialogService.ShowAsync<FavoriteDialog>("Favorite", parameters, options);
        var result = await dialog.Result;

        if (result.Canceled)
        {
            return;
        }

        if (result.Data is ServiceResult<bool> serviceResult && serviceResult.Data)
        {
            data.IsFavorite = !data.IsFavorite;
        }
    }
    
}