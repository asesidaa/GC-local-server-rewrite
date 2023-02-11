using Application.Common.Models;
using Application.Interfaces;
using Domain.Enums;

namespace Application.Game.Card.Management;

public record CardReissueCommand(long CardId) : IRequestWrapper<string>;

public class CardReissueCommandHandler : CardRequestHandlerBase<CardReissueCommand, string>
{
    public CardReissueCommandHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override Task<ServiceResult<string>> Handle(CardReissueCommand request, CancellationToken cancellationToken)
    {
        // TODO: Support actual reissue
        return Task.FromResult(ServiceResult.Failed<string>(ServiceError.NotReissue));
    }
}