namespace Application.Game.Card.OnlineMatching;

public record UploadOnlineMatchingResultCommand(long CardId, string Data) : ICommandWrapper<string>;

public sealed class UploadOnlineMatchingResultCommandHandler
    : ICommandHandlerWrapper<UploadOnlineMatchingResultCommand, string>
{
    private const string XPath = "/root/online_battle_result";
    private readonly IOnlineMatchingService matchingService;

    public UploadOnlineMatchingResultCommandHandler(IOnlineMatchingService matchingService)
    {
        this.matchingService = matchingService;
    }

    public ValueTask<ServiceResult<string>> Handle(
        UploadOnlineMatchingResultCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var dto = request.Data.DeserializeCardData<OnlineMatchingResultRequestDto>();
            var submission = OnlineMatchingXml.ToResult(dto);
            var result = matchingService.SubmitResult(request.CardId, submission);
            if (!result.IsSuccess)
            {
                return ValueTask.FromResult(
                    ServiceResult.Failed<string>(ServiceError.CustomMessage(result.Message!)));
            }

            var response = new OnlineMatchingResultResponseDto { Status = 1 }.SerializeCardData(XPath);
            return ValueTask.FromResult(new ServiceResult<string>(response));
        }
        catch (OnlineMatchingProtocolException ex)
        {
            return ValueTask.FromResult(ServiceResult.Failed<string>(ServiceError.CustomMessage(ex.Message)));
        }
    }
}

public sealed class OnlineMatchingResultResponseDto
{
    [XmlElement(ElementName = "status")]
    public int Status { get; set; }
}
