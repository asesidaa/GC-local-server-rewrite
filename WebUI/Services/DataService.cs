 using Throw;
using WebUI.Common.Models;

namespace WebUI.Services;

public class DataService : IDataService
{
    private Dictionary<uint, Avatar> avatars = new();
    
    private Dictionary<uint, Navigator> navigators = new();
    
    private Dictionary<uint, Title> titles = new();

    private List<Avatar> sortedAvatarList = new();

    private List<Navigator> sortedNavigatorList = new();

    private List<Title> sortedTitleList = new();

    private readonly HttpClient client;

    public DataService(HttpClient client)
    {
        this.client = client;
    }

    public async Task InitializeAsync()
    {
        var avatarList = await client.GetFromJsonAsync<List<Avatar>>("data/Avatars.json");
        avatarList.ThrowIfNull();
        avatars = avatarList.ToDictionary(avatar => avatar.AvatarId);
        sortedAvatarList = avatarList.OrderBy(avatar => avatar.AvatarId).ToList();
        
        var navigatorList = await client.GetFromJsonAsync<List<Navigator>>("data/Navigators.json");
        navigatorList.ThrowIfNull();
        navigators = navigatorList.ToDictionary(navigator => navigator.Id);
        sortedNavigatorList = navigatorList.OrderBy(navigator => navigator.Id).ToList();
        
        var titleList = await client.GetFromJsonAsync<List<Title>>("data/Titles.json");
        titleList.ThrowIfNull();
        titles = titleList.ToDictionary(title => title.Id);
        sortedTitleList = titleList.OrderBy(title => title.Id).ToList();
    }

    public IReadOnlyList<Avatar> GetAvatarsSortedById()
    {
        return sortedAvatarList;
    }

    public IReadOnlyList<Navigator> GetNavigatorsSortedById()
    {
        return sortedNavigatorList;
    }

    public IReadOnlyList<Title> GetTitlesSortedById()
    {
        return sortedTitleList;
    }

    public Avatar? GetAvatarById(uint id)
    {
        return avatars.GetValueOrDefault(id);
    }

    public Title? GetTitleById(uint id)
    {
        return titles.GetValueOrDefault(id);
    }

    public Navigator? GetNavigatorById(uint id)
    {
        return navigators.GetValueOrDefault(id);
    }
}