namespace SharedProject.models;

public class OnlineMatchingUpdateRequest
{
    public long Action { get; set; }
    
    public long EventId { get; set; }

    public long CardId { get; set; }
    
    public long MatchingId { get; set; }

    public long MessageId { get; set; }
}