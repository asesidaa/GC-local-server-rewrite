using Throw;

namespace WebUI.Pages;

public partial class TotalResult
{
    private readonly List<BreadcrumbItem> breadcrumbs = new()
    {
        new BreadcrumbItem("Cards", href: "/Cards")
    };
    
    [Parameter]
    public long CardId { get; set; }
    
    [Inject]
    public required HttpClient Client { get; set; }
    
    private string? errorMessage;

    private TotalResultData? totalResultData;

    protected async override Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        
        breadcrumbs.Add(new BreadcrumbItem($"Card: {CardId}", href:null, disabled:true));
        breadcrumbs.Add(new BreadcrumbItem("TotalResult", href: $"/Cards/TotalResult/{CardId}", disabled: false));

        var result = await Client.GetFromJsonAsync<ServiceResult<TotalResultData>>($"api/Profiles/TotalResult/{CardId}");
        result.ThrowIfNull();
        
        if (!result.Succeeded)
        {
            errorMessage = result.Error!.Message;
            return;
        }

        totalResultData = result.Data;
    }
}