using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SharedProject.common;
using SharedProject.models;

namespace MudAdmin.Pages;

public partial class User
{
    
    [Inject]
    public HttpClient Client { get; set; } = null!;

    [Inject]
    public IDialogService DialogService { get; set; } = null!;

    [Inject]
    public ILogger<User> Logger { get; set; } = null!;
    
    [Parameter]
    public long CardId { get; set; }

    private PlayOption playOption = new();

    private UserDetail? userDetail;

    private List<SongPlayData> songPlayDataList = new();

    private Dictionary<long, Navigator> navigatorDictionary = new();

    private Dictionary<long, Title> titleDictionary = new();

    private Dictionary<long, Avatar> avatarDictionary = new();

    private bool isSavingOptions;

    private int avatarMaxItems = 50;

    private bool pageLoading = true;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        userDetail = await Client.GetFromJsonAsync<UserDetail>($"api/UserDetail/{CardId}");
        if (userDetail is null)
        {
            pageLoading = false;
            return;
        }
        songPlayDataList = userDetail.SongPlayDataList ?? new List<SongPlayData>();
        playOption = userDetail.PlayOption;

        var navigators = await Client.GetFromJsonAsync<Navigators>("data/navigator.json");
        if (navigators?.NavigatorList != null)
        {
            this.navigatorDictionary = navigators.NavigatorList.ToDictionary(navigator => (long)navigator.Id);
        }
        var avatars = await Client.GetFromJsonAsync<Avatar[]>("data/avatar.json");
        if (avatars != null)
        {
            this.avatarDictionary = avatars.ToDictionary(avatar => (long)avatar.Id);
        }
        var titles = await Client.GetFromJsonAsync<Title[]>("data/title.json");
        if (titles != null)
        {
            this.titleDictionary = titles.ToDictionary(title => (long)title.Id);
        }
        pageLoading = false;
    }

    private void OnShowDetailsClick(SongPlayData data)
    {
        data.ShowDetails = !data.ShowDetails;
    }

    private async Task SaveOptions()
    {
        isSavingOptions = true;
        var postData = new PlayOption
        {
            CardId = CardId,
            FastSlowIndicator = playOption.FastSlowIndicator,
            FeverTrance = playOption.FeverTrance,
            AvatarId = playOption.AvatarId,
            NavigatorId = playOption.NavigatorId,
            TitleId = playOption.TitleId
        };
        var result = await Client.PostAsJsonAsync("api/UserDetail/SetPlayOption", postData);
        isSavingOptions = false;
    }

    private static string CalculateRating(int score)
    {
        var grade = SharedConstants.GRADES.Where(g => g.Score <= score).Select(g => g.Grade).Last();
        return grade;
    }

    private async Task OnFavoriteToggled(SongPlayData data)
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = false,
            DisableBackdropClick = true,
            FullWidth = true
        };
        var parameters = new DialogParameters();
        parameters.Add("Data", data);
        parameters.Add("CardId", CardId);
        var dialog = DialogService.Show<FavoriteDialog>("Favorite", parameters, options);
        var result = await dialog.Result;

        if (result.Cancelled)
        {
            return;
        }

        if ((bool)result.Data)
        {
            Logger.LogInformation("Changed!");
            data.IsFavorite = !data.IsFavorite;
        }
    }

    private Task<IEnumerable<long>> SearchAvatar(string value)
    {
        var result = string.IsNullOrEmpty(value) ?
            avatarDictionary.Keys :
            avatarDictionary.Where(pair => pair.Value.ToString().Contains(value, StringComparison.InvariantCultureIgnoreCase)).Select(pair => pair.Key);
        return Task.FromResult(result);
    }
    
    private Task<IEnumerable<long>> SearchTitle(string value)
    {
        var result = string.IsNullOrEmpty(value) ?
            titleDictionary.Keys :
            titleDictionary.Where(pair => pair.Value.ToString().Contains(value, StringComparison.InvariantCultureIgnoreCase)).Select(pair => pair.Key);
        return Task.FromResult(result);
    }
    
    private Task<IEnumerable<long>> SearchNavigator(string value)
    {
        var result = string.IsNullOrEmpty(value) ?
            navigatorDictionary.Keys :
            navigatorDictionary.Where(pair => pair.Value.ToString().Contains(value, StringComparison.InvariantCultureIgnoreCase)).Select(pair => pair.Key);
        return Task.FromResult(result);
    }

    private string AvatarIdToString(long id)
    {
        return avatarDictionary.ContainsKey(id) ? avatarDictionary[id].ToString() : $"No Data for {id}!";
    }
    
    private string NavigatorIdToString(long id)
    {
        return navigatorDictionary.ContainsKey(id) ? navigatorDictionary[id].ToString() : $"No Data for {id}!";
    }
    
    private string TitleIdToString(long id)
    {
        return titleDictionary.ContainsKey(id) ? titleDictionary[id].ToString() : $"No Data for {id}!";
    }
}