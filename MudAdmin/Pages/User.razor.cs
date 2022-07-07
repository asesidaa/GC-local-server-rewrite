using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using ProtoBuf;
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

    private Dictionary<long, Avatar> avatarDictionary = new();

    private bool isSavingOptions;

    private Dictionary<long, Navigator> navigatorDictionary = new();

    private bool pageLoading = true;

    private PlayOption playOption = new();

    private List<SongPlayData> songPlayDataList = new();

    private Dictionary<long, Title> titleDictionary = new();

    private UserDetail? userDetail;

    [Parameter]
    public long CardId { get; set; }

    private bool IsTitleOverlayVisible { get; set; }

    private bool IsNavigatorOverlayVisible { get; set; }

    private bool IsAvatarOverlayVisible { get; set; }

    private Title? SelectedTitle { get; set; }
    
    private Avatar? SelectedAvatar { get; set; }
    
    private Navigator? SelectedNavigator { get; set; }

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

        var navigatorStream = await Client.GetStreamAsync(SharedConstants.NAVIGATOR_DAT_URI);
        var navigators = Serializer.Deserialize<Navigators>(navigatorStream);
        if (navigators?.NavigatorList != null)
        {
            navigatorDictionary = navigators.NavigatorList.ToDictionary(navigator => (long)navigator.Id);
        }

        var avatarStream = await Client.GetStreamAsync(SharedConstants.AVATAR_DAT_URI);
        var avatars = Serializer.Deserialize<List<Avatar>>(avatarStream);
        if (avatars != null)
        {
            avatarDictionary = avatars.ToDictionary(avatar => (long)avatar.Id);
        }

        var titleStream = await Client.GetStreamAsync(SharedConstants.TITLE_DAT_URI);
        var titles = Serializer.Deserialize<List<Title>>(titleStream);
        if (titles != null)
        {
            titleDictionary = titles.ToDictionary(title => (long)title.Id);
        }
        
        SetSelected();
        pageLoading = false;
    }

    private void SetSelected()
    {
        SelectedTitle = titleDictionary.GetValueOrDefault(playOption.TitleId);
        SelectedAvatar = avatarDictionary.GetValueOrDefault(playOption.AvatarId);
        SelectedNavigator = navigatorDictionary.GetValueOrDefault(playOption.NavigatorId);
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
        var grade = SharedConstants.Grades.Where(g => g.Score <= score).Select(g => g.Grade).Last();
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
        var result = string.IsNullOrEmpty(value) ? avatarDictionary.Keys : avatarDictionary.Where(pair => pair.Value.ToString().Contains(value, StringComparison.InvariantCultureIgnoreCase)).Select(pair => pair.Key);
        return Task.FromResult(result);
    }

    private Task<IEnumerable<long>> SearchTitle(string value)
    {
        var result = string.IsNullOrEmpty(value) ? titleDictionary.Keys : titleDictionary.Where(pair => pair.Value.ToString().Contains(value, StringComparison.InvariantCultureIgnoreCase)).Select(pair => pair.Key);
        return Task.FromResult(result);
    }

    private Task<IEnumerable<long>> SearchNavigator(string value)
    {
        var result = string.IsNullOrEmpty(value) ? navigatorDictionary.Keys : navigatorDictionary.Where(pair => pair.Value.ToString().Contains(value, StringComparison.InvariantCultureIgnoreCase)).Select(pair => pair.Key);
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

    private void OnChangeTitleButtonClick()
    {
        IsTitleOverlayVisible = true;
    }

    private void OnChangeNavigatorButtonClick()
    {
        IsNavigatorOverlayVisible = true;
    }

    private void OnChangeAvatarButtonClick()
    {
        IsAvatarOverlayVisible = true;
    }

    private void OnTitleOverlayClosed()
    {
        IsTitleOverlayVisible = false;
        if (SelectedTitle != null)
        {
            playOption.TitleId = SelectedTitle.Id;
        }
    }

    private void OnNavigatorOverlayClosed()
    {
        IsNavigatorOverlayVisible = false;
        if (SelectedNavigator != null)
        {
            playOption.NavigatorId = SelectedNavigator.Id;
        }
    }

    private void OnAvatarOverlayClosed()
    {
        IsAvatarOverlayVisible = false;
        if (SelectedAvatar != null)
        {
            playOption.AvatarId = SelectedAvatar.Id;
        }
    }

    private static object NameEntrySortByFunc(Navigator navigator)
    {
        return navigator.NameEntry1?.ToString().ToLowerInvariant() ?? string.Empty;
    }
}