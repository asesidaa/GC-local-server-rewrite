using System.Text;
using MediatR;

namespace Application.Game.Server;

public record GetDataQuery(string Host, string Scheme) : IRequest<string>;

public class GetDataQueryHandler : IRequestHandler<GetDataQuery, string>
{
    private readonly IEventManagerService eventManagerService;

    public GetDataQueryHandler(IEventManagerService eventManagerService)
    {
        this.eventManagerService = eventManagerService;
    }

    public Task<string> Handle(GetDataQuery request, CancellationToken cancellationToken)
    {
        var response = "count=0\n" +
                       "nexttime=180";
        if (!eventManagerService.UseEvents())
        {
            return Task.FromResult(response);
        }
        
        var urlBase = $"{request.Scheme}://{request.Host}/events/";
        var dataString = new StringBuilder();
        var events = eventManagerService.GetEvents();
        var count = 0;
        foreach (var pair in events.Select((@event, i) => new {Value = @event, Index = i}))
        {
            var value = pair.Value;
            var index = pair.Index;
            var fileUrl = $"{urlBase}{value.Name}";
            var eventString = $"{index},{fileUrl},{value.NotBefore},{value.NotAfter},{value.Md5},{value.Index}";
            dataString.Append(eventString).Append('\n');
            count++;
        }

        response = $"count={count}\n" +
                   "nexttime=180\n" +
                   $"{dataString}";

        return Task.FromResult(response);
    }
}