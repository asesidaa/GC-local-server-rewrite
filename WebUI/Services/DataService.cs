using Throw;
using WebUI.Common.Models;

namespace WebUI.Services;

public class DataService : IDataService
{
    private Dictionary<uint, Avatar> avatars = new();
    private Dictionary<uint, Navigator> navigators = new();
    private Dictionary<uint, Title> titles = new();
    private Dictionary<uint, Item> items = new();
    private Dictionary<uint, Skin> skins = new();
    private Dictionary<uint, SoundEffect> soundEffects = new();

    private List<Avatar> sortedAvatarList = new();
    private List<Navigator> sortedNavigatorList = new();
    private List<Title> sortedTitleList = new();
    private List<Item> sortedItemList = new();
    private List<Skin> sortedSkinList = new();
    private List<SoundEffect> sortedSoundEffectList = new();

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

        var itemList = await client.GetFromJsonAsync<List<Item>>("data/Items.json");
        itemList.ThrowIfNull();
        items = itemList.ToDictionary(item => item.Id);
        sortedItemList = itemList.OrderBy(item => item.Id).ToList();

        var skinList = await client.GetFromJsonAsync<List<Skin>>("data/Skins.json");
        skinList.ThrowIfNull();
        skins = skinList.ToDictionary(skin => skin.Id);
        sortedSkinList = skinList.OrderBy(skin => skin.Id).ToList();

        var soundEffectList = await client.GetFromJsonAsync<List<SoundEffect>>("data/SoundEffects.json");
        soundEffectList.ThrowIfNull();
        soundEffects = soundEffectList.ToDictionary(se => se.Id);
        sortedSoundEffectList = soundEffectList.OrderBy(se => se.Id).ToList();
    }

    public IReadOnlyList<Avatar> GetAvatarsSortedById() => sortedAvatarList;
    public IReadOnlyList<Navigator> GetNavigatorsSortedById() => sortedNavigatorList;
    public IReadOnlyList<Title> GetTitlesSortedById() => sortedTitleList;
    public IReadOnlyList<Item> GetItemsSortedById() => sortedItemList;
    public IReadOnlyList<Skin> GetSkinsSortedById() => sortedSkinList;
    public IReadOnlyList<SoundEffect> GetSoundEffectsSortedById() => sortedSoundEffectList;

    public Avatar? GetAvatarById(uint id) => avatars.GetValueOrDefault(id);
    public Title? GetTitleById(uint id) => titles.GetValueOrDefault(id);
    public Navigator? GetNavigatorById(uint id) => navigators.GetValueOrDefault(id);
    public Item? GetItemById(uint id) => items.GetValueOrDefault(id);
    public Skin? GetSkinById(uint id) => skins.GetValueOrDefault(id);
    public SoundEffect? GetSoundEffectById(uint id) => soundEffects.GetValueOrDefault(id);
}
