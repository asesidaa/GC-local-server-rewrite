namespace Application.Game.Card.OnlineMatching;

public record UploadOnlineMatchingResultCommand(long CardId, string Data) : ICommandWrapper<string>;

public class UploadOnlineMatchingResultCommandHandler : ICommandHandlerWrapper<UploadOnlineMatchingResultCommand, string>
{
    private const string XPATH = "/root/online_battle_result";

    public ValueTask<ServiceResult<string>> Handle(UploadOnlineMatchingResultCommand request, CancellationToken cancellationToken)
    {
        var result = new OnlineMatchingResult { Status = 1 }.SerializeCardData(XPATH);
        return ValueTask.FromResult(new ServiceResult<string>(result));
    }
}

public class OnlineMatchingResult
{
    [XmlElement(ElementName = "status")]
    public int Status { get; set; }
}
