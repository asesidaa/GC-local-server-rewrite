using Domain.Models;

namespace Application.Interfaces;

public interface IEventManagerService
{
    public void InitializeEvents();

    public bool UseEvents();

    public IEnumerable<Event> GetEvents();
}