namespace Application.Game.Card.OnlineMatching;

public record UploadOnlineMatchingResultCommand(long CardId, string Data) : IRequestWrapper<string>;

public class UploadOnlineMatchingResultCommandHandler : RequestHandlerBase<UploadOnlineMatchingResultCommand, string>
{
    private const string XPATH = "/root/online_battle_result";
    
    public UploadOnlineMatchingResultCommandHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override Task<ServiceResult<string>> Handle(UploadOnlineMatchingResultCommand request, CancellationToken cancellationToken)
    {
        var result = new OnlineMatchingResult { Status = 1 }.SerializeCardData(XPATH);
        return Task.FromResult(new ServiceResult<string>(result));
    }
}

public class OnlineMatchingResult
{
    [XmlElement(ElementName = "status")]
    public int Status { get; set; }
}