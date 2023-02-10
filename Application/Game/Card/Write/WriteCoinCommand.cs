using Application.Common.Models;
using Application.Interfaces;

namespace Application.Game.Card.Write;

public record WriteCoinCommand(long CardId, string Data) : IRequestWrapper<string>;

public class WriteCoinCommandHandler : CardRequestHandlerBase<WriteCoinCommand, string>
{
    public WriteCoinCommandHandler(ICardDependencyAggregate aggregate) : base(aggregate) {}

    public override Task<ServiceResult<string>> Handle(WriteCoinCommand request, CancellationToken cancellationToken)
    {
        // TODO: Add proper implementation
        return Task.FromResult(new ServiceResult<string>(request.Data));
    }
}