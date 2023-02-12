using Domain.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Interfaces;

public interface ICardDependencyAggregate
{
    ICardDbContext CardDbContext { get; }
    IMusicDbContext MusicDbContext { get; }
    
    IOptions<GameConfig> Options { get; }
}