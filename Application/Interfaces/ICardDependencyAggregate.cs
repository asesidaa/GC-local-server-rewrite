using Microsoft.Extensions.Logging;

namespace Application.Interfaces;

public interface ICardDependencyAggregate
{
    ICardDbContext CardDbContext { get; }
    IMusicDbContext MusicDbContext { get; }
}