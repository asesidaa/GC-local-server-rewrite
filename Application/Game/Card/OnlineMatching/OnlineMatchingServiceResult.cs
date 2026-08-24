namespace Application.Game.Card.OnlineMatching;

public enum OnlineMatchingServiceError
{
    InvalidIdentity,
    IdentityConflict,
    RoomNotFound,
    EventMismatch,
    NotMember,
    InvalidAction,
    InvalidPhase,
    ConflictingResult
}

public sealed record OnlineMatchingServiceResult<T>(
    bool IsSuccess,
    T? Value,
    OnlineMatchingServiceError? Error,
    string? Message)
{
    public static OnlineMatchingServiceResult<T> Success(T value) => new(true, value, null, null);

    public static OnlineMatchingServiceResult<T> Failure(OnlineMatchingServiceError error, string message) =>
        new(false, default, error, message);
}
