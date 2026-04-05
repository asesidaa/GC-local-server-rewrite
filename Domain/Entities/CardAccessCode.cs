namespace Domain.Entities;

public class CardAccessCode
{
    public long CardId { get; set; }

    public string HashedCode { get; set; } = string.Empty;
}
