using System.Net;
using GCRelayServer.Protocol;

namespace GCRelayServer.Routing;

public enum RelayRegistrationOutcome
{
    Added,
    Refreshed,
    Rejected
}

public sealed record RelayRegistrationResult(
    RelayRegistrationOutcome Outcome,
    RelayMember? Member,
    IReadOnlyList<RelayMember> ExistingMembers,
    RelayRejection? Rejection)
{
    public static RelayRegistrationResult Rejected(RelayRejection rejection) =>
        new(RelayRegistrationOutcome.Rejected, null, [], rejection);
}

public sealed record RelayMembershipOperationResult(bool IsSuccess, RelayMember? Member, RelayRejection? Rejection)
{
    public static RelayMembershipOperationResult Success(RelayMember member) => new(true, member, null);

    public static RelayMembershipOperationResult Failure(RelayRejection rejection) => new(false, null, rejection);
}

public sealed class RelayRoom
{
    private const int MaximumMembers = 4;
    private readonly Lock gate = new();
    private readonly Dictionary<uint, RelayMember> membersByMachine = [];
    private readonly Dictionary<uint, RelayMember> membersByEntry = [];
    private DateTimeOffset lastActivityAt;

    internal RelayRoom(uint matchingId, DateTimeOffset now)
    {
        MatchingId = matchingId;
        lastActivityAt = now;
    }

    public uint MatchingId { get; }

    public DateTimeOffset LastActivityAt
    {
        get
        {
            lock (gate)
            {
                return lastActivityAt;
            }
        }
    }

    public RelayRegistrationResult Register(
        IPEndPoint endpoint,
        RelayDatagram datagram,
        RelayProfileIdentity profileIdentity,
        DateTimeOffset now)
    {
        lock (gate)
        {
            if (membersByMachine.TryGetValue(datagram.SenderMachineId, out var existing))
            {
                if (existing.EntryNumber != datagram.EntryNumber || existing.ProfileIdentity != profileIdentity)
                {
                    return RelayRegistrationResult.Rejected(new RelayRejection(
                        RelayRejectionCode.MemberConflict,
                        "Known machine changed its entry or stable profile identity."));
                }

                if (!existing.IsEndpoint(endpoint))
                {
                    return RelayRegistrationResult.Rejected(new RelayRejection(
                        RelayRejectionCode.SenderEndpointMismatch,
                        "Known machine sent from another UDP endpoint."));
                }

                existing.Refresh(datagram.Payload.Span, now);
                lastActivityAt = now;
                return new RelayRegistrationResult(
                    RelayRegistrationOutcome.Refreshed,
                    existing,
                    membersByMachine.Values.Where(member => member.MachineId != existing.MachineId).ToArray(),
                    null);
            }

            if (membersByMachine.Count >= MaximumMembers)
            {
                return RelayRegistrationResult.Rejected(new RelayRejection(
                    RelayRejectionCode.RoomFull,
                    "A channel-6 room cannot exceed four members."));
            }

            if (membersByEntry.ContainsKey(datagram.EntryNumber))
            {
                return RelayRegistrationResult.Rejected(new RelayRejection(
                    RelayRejectionCode.MemberConflict,
                    $"Entry {datagram.EntryNumber} already belongs to another machine."));
            }

            var existingMembers = membersByMachine.Values.OrderBy(member => member.EntryNumber).ToArray();
            var added = new RelayMember(
                datagram.SenderMachineId,
                datagram.EntryNumber,
                endpoint,
                profileIdentity,
                datagram.Payload.Span,
                now);
            membersByMachine.Add(added.MachineId, added);
            membersByEntry.Add(added.EntryNumber, added);
            lastActivityAt = now;
            return new RelayRegistrationResult(RelayRegistrationOutcome.Added, added, existingMembers, null);
        }
    }

    public RelayMembershipOperationResult Remove(IPEndPoint endpoint, RelayDatagram datagram, DateTimeOffset now)
    {
        lock (gate)
        {
            if (!membersByMachine.TryGetValue(datagram.SenderMachineId, out var member) ||
                member.EntryNumber != datagram.EntryNumber)
            {
                return RelayMembershipOperationResult.Failure(new RelayRejection(
                    RelayRejectionCode.SenderNotRegistered,
                    "Leave sender is not registered with this machine and entry."));
            }

            if (!member.IsEndpoint(endpoint))
            {
                return RelayMembershipOperationResult.Failure(new RelayRejection(
                    RelayRejectionCode.SenderEndpointMismatch,
                    "Leave sender endpoint does not match registration."));
            }

            membersByMachine.Remove(member.MachineId);
            membersByEntry.Remove(member.EntryNumber);
            lastActivityAt = now;
            return RelayMembershipOperationResult.Success(member);
        }
    }

    public IReadOnlyList<RelayMember> GetMembers()
    {
        lock (gate)
        {
            return membersByMachine.Values.OrderBy(member => member.EntryNumber).ToArray();
        }
    }

    public bool IsEmpty
    {
        get
        {
            lock (gate)
            {
                return membersByMachine.Count == 0;
            }
        }
    }

    public bool IsExpired(DateTimeOffset now, TimeSpan idleExpiry)
    {
        lock (gate)
        {
            return now - lastActivityAt >= idleExpiry;
        }
    }
}
