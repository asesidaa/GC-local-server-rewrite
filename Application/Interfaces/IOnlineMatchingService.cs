using Application.Game.Card.OnlineMatching;
using Domain.Models.OnlineMatching;

namespace Application.Interfaces;

public interface IOnlineMatchingService
{
    OnlineMatchingServiceResult<OnlineMatchRoomSnapshot> Enter(OnlineMatchParticipant participant);

    OnlineMatchingServiceResult<OnlineMatchRoomSnapshot> Update(
        uint matchingId,
        uint eventId,
        long cardId,
        uint action,
        uint messageId);

    OnlineMatchingServiceResult<bool> SubmitResult(long cardId, OnlineMatchResultSubmission submission);

    void CleanupStaleRooms();
}
