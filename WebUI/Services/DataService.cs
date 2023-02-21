using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Throw;
using WebUI.Common.Models;

namespace WebUI.Services;

public class DataService : IDataService
{
    private Dictionary<uint, Avatar> avatars = new();
    
    private Dictionary<uint, Navigator> navigators = new();
    
    private Dictionary<uint, Title> titles = new();

    private readonly HttpClient client;

    public DataService(HttpClient client)
    {
        this.client = client;
    }

    public async Task InitializeAsync()
    {
        var avatarList = await client.GetFromJsonAsync("data/Avatars.json", SourceGenerationContext.Default.ListAvatar);
        avatarList.ThrowIfNull();
        avatars = avatarList.ToDictionary(avatar => avatar.AvatarId);
        
        var navigatorList = await client.GetFromJsonAsync("data/Navigators.json", SourceGenerationContext.Default.ListNavigator);
        navigatorList.ThrowIfNull();
        navigators = navigatorList.ToDictionary(navigator => navigator.Id);
        
        var titleList = await client.GetFromJsonAsync("data/Titles.json", SourceGenerationContext.Default.ListTitle);
        titleList.ThrowIfNull();
        titles = titleList.ToDictionary(title => title.Id);
    }

    public IReadOnlyDictionary<uint, Avatar> GetAvatars()
    {
        return new ReadOnlyDictionary<uint, Avatar>(avatars);
    }

    public IReadOnlyDictionary<uint, Navigator> GetNavigators()
    {
        return new ReadOnlyDictionary<uint, Navigator>(navigators);
    }

    public IReadOnlyDictionary<uint, Title> GetTitles()
    {
        return new ReadOnlyDictionary<uint, Title>(titles);
    }
}

[JsonSerializable(typeof(List<Avatar>))]
[JsonSerializable(typeof(List<Navigator>))]
[JsonSerializable(typeof(List<Title>))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
    
}