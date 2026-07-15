using WebUI.Common.Models;

namespace WebUI.Services;

public interface IDataService
{
    public Task InitializeAsync();

    public IReadOnlyList<Avatar> GetAvatarsSortedById();
    public IReadOnlyList<Navigator> GetNavigatorsSortedById();
    public IReadOnlyList<Title> GetTitlesSortedById();
    public IReadOnlyList<Item> GetItemsSortedById();
    public IReadOnlyList<Skin> GetSkinsSortedById();
    public IReadOnlyList<SoundEffect> GetSoundEffectsSortedById();

    public Avatar? GetAvatarById(uint id);
    public Title? GetTitleById(uint id);
    public Navigator? GetNavigatorById(uint id);
    public Item? GetItemById(uint id);
    public Skin? GetSkinById(uint id);
    public SoundEffect? GetSoundEffectById(uint id);
}
