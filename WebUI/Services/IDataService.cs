using System.Collections.Immutable;
using WebUI.Common.Models;

namespace WebUI.Services;

public interface IDataService
{
    public Task InitializeAsync();

    public IReadOnlyDictionary<uint, Avatar> GetAvatars();

    public IReadOnlyDictionary<uint, Navigator> GetNavigators();

    public IReadOnlyDictionary<uint, Title> GetTitles();
}