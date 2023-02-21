using System.Collections.Immutable;
using WebUI.Common.Models;

namespace WebUI.Services;

public interface IDataService
{
    public Task InitializeAsync();

    public IReadOnlyList<Avatar> GetAvatarsSortedById();

    public IReadOnlyList<Navigator> GetNavigatorsSortedById();

    public IReadOnlyList<Title> GetTitlesSortedById();

    public Avatar? GetAvatarById(uint id);
    
    public Title? GetTitleById(uint id);
    
    public Navigator? GetNavigatorById(uint id);
}