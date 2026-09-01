# Multiplayer Pipeline Redesign

**Date:** 2026-08-25

**Status:** Approved

## Context

The current online-matching implementation models the card-service room and
the UDP relay as loosely related endpoint lists. Reverse engineering of
`game471.exe` shows a richer but still clean separation:

- MainServer owns event-scoped roster allocation and service-controlled timing
  through card request types `8705`, `8961`, and `8709`.
- GCRelayServer independently reconstructs transient transport membership from
  channel-6 `A6`/`A0` traffic and routes gameplay by the shared
  `matching_id`.
- The game owns separate LAN discovery and direct-play paths on UDP/60100
  channel 3 and UDP/60200 channels 4/5. They are not relay responsibilities.

The definitive wire and lifecycle contract is
[`doc/multiplayer/game471-multiplayer-protocol.md`](../../../doc/multiplayer/game471-multiplayer-protocol.md).
The address-level reverse-engineering ledger is
[`doc/multiplayer/game471-multiplayer-research.md`](../../../doc/multiplayer/game471-multiplayer-research.md).

## Goals

- Preserve MainServer and GCRelayServer as separately deployable processes.
- Make MainServer matching event-scoped, idempotent, time-based, and faithful
  to the native request/response contract.
- Replace the relay's mutable reflection serializer and endpoint-only rooms
  with an explicit, validated big-endian protocol model.
- Implement the complete channel-6 membership and routed-message inventory.
- Make timers and room expiry deterministic under tests.
- Parse and associate online result uploads without introducing an unrelated
  persistent analytics feature.
- Record protocol decisions and evidence boundaries so future work does not
  have to rediscover the binary.

## Non-goals

- Combining MainServer and GCRelayServer into one executable or shared runtime
  service.
- Relaying LAN discovery (channel 3), direct setup (channel 4), or direct
  gameplay (channel 5).
- Persisting active matching rooms across process restarts.
- Adding durable online-result history, rankings, or WebUI management pages.
- Claiming the provisional `99`/`40` timing policy is an exact reconstruction
  of the retired official service.
- Renaming opaque fields in the native `0050` result record without evidence.

## Architectural boundary

The processes share protocol invariants, not memory or service calls:

```text
                         certification relay_addr/relay_port
MainServer ----------------------------------------------------------+
    |                                                               |
    | 8705: allocate event room, uint32 matching_id, entry_no        |
    | 8961: validate participant, accept action 0, return state      |
    | 8709: validate and associate result                            |
    v                                                               v
game471.exe  -----------------------------------------------> GCRelayServer
               UDP/60200 channel 6, matching_id + machine_id
```

This preserves independent deployment and failure domains. It also matches the
client: HTTP installs the roster first, while the relay learns UDP endpoints
later from profile-bearing packets.

## MainServer design

### Layering

- `Domain` owns the matching phase enum, room aggregate, participant model,
  result model, and validated timing/retention configuration.
- `Application` owns XML request/response DTOs, Mediator handlers, the service
  contract, and conversion to card-service XML.
- `Infrastructure` owns the singleton in-memory coordinator, matching-ID
  allocation, `TimeProvider`, indexes, logging, and cleanup implementation.
- `MainServer` binds and validates configuration at startup and retains the
  existing `CardController` routes.

This removes transport serialization concerns from the room aggregate and
keeps persistence out of an intentionally transient feature.

### Room aggregate

An online room has:

- a nonzero `uint` matching ID;
- one `event_id`;
- a phase: `Searching`, `Forming`, `Ready`, or `Closed`;
- up to four participants with stable entry numbers `0..3`;
- creation, phase-deadline, last-activity, ready, and closed timestamps;
- per-participant result-received state.

The aggregate accepts `DateTimeOffset now` for every operation. It does not
read the system clock directly. The Infrastructure service supplies
`TimeProvider.GetUtcNow()`, allowing tests to advance time without sleeping.

All phase advancement happens before admission, snapshot creation, update, or
cleanup decisions. A room that has reached its deadline cannot accidentally
accept another participant merely because no client polled at the exact
deadline.

### Admission and idempotency

`8705` admission is serialized by the singleton coordinator:

1. Validate that `machine_id` and `event_id` fit nonzero unsigned 32-bit fields
   and that required profile values are usable.
2. If the same card, machine, and event already belong to a non-closed room,
   treat the request as a retry and return the same matching ID and entry
   number. Mutable `message_id` may refresh; conflicting stable profile data is
   rejected.
3. Reject a conflicting active identity: the same card with another machine,
   the same machine with another card, or a duplicate entry identity within a
   room.
4. Advance existing rooms for the requested event and select the oldest
   joinable `Searching` or `Forming` room.
5. If none is joinable, allocate a cryptographically seeded, nonzero `uint`
   matching ID that is absent from active rooms and retained tombstones.
6. Assign the lowest unused entry number. Entry numbers never shift after a
   participant joins.

An identity remains reserved through result submission or room expiry so a
network retry cannot become a second match. Successful terminal result
handling releases it for a later session.

Matching IDs are held in a short-lived tombstone set after room removal. This
prevents immediate reuse while delayed HTTP or UDP packets may still exist.
Random allocation makes accidental reuse across a process restart negligible
without adding persistent matching state.

### Timing and phases

The default policy is configurable under `OnlineMatching` in
`MainServer/Configurations/matching.json`:

```json
{
  "Relay": {
    "RelayServer": "127.0.0.1",
    "RelayPort": 3333
  },
  "OnlineMatching": {
    "SearchTimeoutSeconds": 99,
    "FormationTimeoutSeconds": 40,
    "PollIntervalSeconds": 10,
    "RoomIdleExpirySeconds": 600,
    "TerminalRetentionSeconds": 120,
    "MatchingIdReuseGuardSeconds": 600,
    "MaxPlayers": 4
  }
}
```

Options are bound with data-annotation validation and `ValidateOnStart`.
`MaxPlayers` is constrained to `1..4`; four remains the protocol default and
maximum.

Phase transitions are:

| From | Event | To | Deadline behavior |
| --- | --- | --- | --- |
| new room | first participant | `Searching` | `now + SearchTimeoutSeconds` |
| `Searching` | second unique participant | `Forming` | replace deadline with `now + FormationTimeoutSeconds` |
| `Searching` | search deadline | `Ready` | continue with the single participant |
| `Forming` | formation deadline | `Ready` | continue with the accumulated two or three participants |
| active | room reaches configured capacity | `Ready` | immediate |
| `Ready` | every participant submitted a result | `Closed` | retain briefly for idempotency |
| any non-closed phase | idle expiry | `Closed`/removed | release indexes and retain ID tombstone |

Additional participants do not restart the 40-second formation window. The
formation deadline begins with the second participant, matching the observed
"someone matched, then wait an additional window" behavior.

### Response timing fields

For a snapshot at time `now`, active rooms derive one shared timing tuple:

```text
timeout   = ceil(max(0, phase_deadline - now) in seconds)
wait      = min(PollIntervalSeconds, timeout)
remaining = max(0, timeout - wait)
```

Every record in the same response receives the same tuple. This yields the
historical-compatible initial shape `99/10/89` without using polling calls as a
clock. `Searching` and `Forming` return `status = 1`; `Ready` and `Closed`
return `status = 3` with zero timing values. The client exits polling on status
3 before using another wait interval.

These field semantics are explicit server policy. A future official packet
capture may change the defaults or mapping without changing the aggregate's
elapsed-time model.

### `8961` update validation

The update request gets a dedicated DTO containing only `action`, `event_id`,
`matching_id`, and `message_id`. The handler/coordinator validates:

- the matching ID exists and event ID agrees with the room;
- the calling card belongs to that room;
- `action = 0`, as initialized and emitted by the audited native class;
- the request cannot change machine, entry, or stable profile identity.

Runtime correction, 2026-09-02: a one-player cabinet sent 18 consecutive
`action = 0` updates. The original entry-dependent `1`/`2` interpretation was
wrong and prevented the elapsed-time timeout from being returned to the client.
The implementation and regression tests now accept only the native zero value.
Entry role remains validated from the server-owned room roster, not from this
request word.

Only the caller's `message_id` and last-activity time change. An absent or
invalid caller cannot advance or mutate the room. Time progression depends
only on `TimeProvider`.

### `8709` result handling

The result request gets a typed DTO for every binary-confirmed field. Its nine
ordinal tags are exactly `music_id_1st/2nd/3rd`,
`difficulty_lv_1st/2nd/3rd`, and `item_id_1st/2nd/3rd`; the pre-redesign model
already had those names, and shortened numeric suffixes are invalid. The
handler validates matching ID, event ID, card membership, and numeric field
ranges, then associates the submission with the participant in the in-memory
room. Repeated identical submissions are idempotent; a conflicting second
submission is rejected and logged.

The aggregate retains the parsed submission only until terminal cleanup. No
database migration or ranking update is introduced. The response remains the
native-compatible `<online_battle_result><status>1</status>` shape after a
successful association.

### Cleanup

The existing Quartz job remains the scheduled owner of MainServer cleanup.
Cleanup advances room phases, removes rooms after idle or terminal retention,
releases active identity indexes, and expires matching-ID tombstones. Creation
time alone is not an expiry signal.

## GCRelayServer design

### Components

The relay is split into independently testable layers:

| Component | Responsibility |
| --- | --- |
| `RelayDatagram` | Immutable semantic representation of the channel-6 header and payload. |
| `RelayDatagramCodec` | Exact big-endian parse/write of the 12-byte outer and 24-byte routed headers. |
| `RelayProtocol` | Descriptor table for subtype, direction, mode, exact payload size, and sender role. |
| `RelayRoom` / `RelayMember` | Locked membership keyed by machine and entry, endpoint binding, stable profile fingerprint, and activity time. |
| `RelayRoomRegistry` | Matching-ID room lookup, capacity, expiry, and removal. |
| `RelayRouter` | Membership state machine and target-selection policy; returns outbound transmissions without touching a socket. |
| `RelayServer` | Thin NetCoreServer adapter that receives bytes, invokes codec/router, sends returned datagrams, and logs rejection reasons. |

`BinarySerializer` is removed. The format is small, fixed, and security
sensitive; direct `BinaryPrimitives` code makes every offset and length check
visible.

### Codec and protocol validation

The validation table deliberately describes the relay's accepted game471
emission contract. The native outer receiver itself does not check the outer
channel parameter/reserved dword, and its routed decoder does not enforce every
subtype mode or payload length. Those stricter relay checks are compatible
allowlisting and hardening, not claims about native receiver validation or the
retired official relay.

Parsing copies the received payload out of NetCoreServer's buffer and rejects:

- datagrams shorter than 36 bytes or larger than the native 8,192-byte receive
  limit;
- `header_size != 12`;
- a non-exact outer `remaining_size`;
- any channel other than 6;
- nonzero reserved outer fields produced as zero by native builders;
- a payload size that does not make total length exactly `36 + payload_size`;
- an unknown subtype;
- relay-generated response types (`A1`, `A2`, `B1`) received from a client;
- an invalid mode or exact payload size for the subtype;
- a target policy inconsistent with membership/control semantics.

Known channel-6 payload rules are encoded once in `RelayProtocol` and used by
both parsing and routing. In particular, `0040` is 84 bytes in host mode `1`
and 24 bytes in guest mode `2`; its scalars remain opaque little-endian bytes.

Serialization allocates the exact output size and writes a fresh immutable
datagram. Acknowledgement construction never mutates the received object.

### Membership identity

Each room is keyed by a nonzero matching ID and caps membership at four. Each
member records:

- machine ID;
- stable entry number `0..3`;
- UDP source endpoint;
- a fingerprint of stable profile identity (`event_id` and `card_id`) from the
  256-byte registration payload;
- the latest profile payload;
- last-seen timestamp.

Machine ID, entry number, and card fingerprint must each be unique within a
room, and all members must carry the same nonzero event ID. A known member's
later datagram must come from its bound endpoint and use the same entry number.
Profile reannouncements may update dynamic profile words but may not replace
the stable identity fingerprint. The relay does not attempt to make the UDP
profile more authoritative than HTTP; the fingerprint is an anti-confusion
check within a transient room.

A rejected fifth member leaves all four existing members untouched. Room
capacity never resets or clears a live room.

### Routing state machine

This state machine is reconstructed compatibility policy derived from native
client emission order and receive transitions. The official relay
implementation is absent from the game binary, so exact official admission,
broadcast, fallback, and acknowledgement-duplication policy remains externally
unverified.

The router applies these rules:

| Inbound | Required state | Result |
| --- | --- | --- |
| `B0` | no room membership | Send exactly one `B1` to the source. |
| new `A6` | valid unused machine/entry and capacity | Register; send the new profile as `A2` to existing members. |
| known `A6` | exact member identity | Update dynamic profile/activity; forward `A6` to every other member. |
| new `A0` | same checks as `A6` | Loss-recovery registration; send `A2` mode 4 to existing members, then one `A1` to the source. |
| known `A0` | exact member identity | Refresh activity/profile; send one `A1` to the source. |
| `A3` | exact member identity | Remove sender; consume packet. |
| `A7` | registered entry-zero host | Broadcast to all other members. |
| gameplay/control broadcast | registered sender and valid role | Target `0`/`FFFFFFFF`: all other members. |
| gameplay/control unicast | registered sender and valid role | Concrete target: that member only. |

Role validation follows the native modes: entry zero is host for host-only
`0030` mode 1, `0040` mode 1, `0071`, and `A7`; nonzero entries use guest-only
`0030` mode 2, `0040` mode 2, and `0070`. Other routed gameplay messages are
accepted from any registered member when their descriptor matches.

The `A0` loss-recovery path constructs `A2` with mode 4 even though inbound
`A0` uses mode 0. Acknowledgements preserve the matching, entry, sender, route,
and payload fields required by the native handler while changing only the
response subtype (and the `A2` mode where required).

### Expiry and process lifecycle

Relay room idle expiry remains a configurable ten-minute compatibility policy.
The standalone process uses `TimeProvider` and a `PeriodicTimer` cleanup loop
so rooms expire even when no later packet arrives. Command-line configuration
retains the positional port form and adds named values for room expiry and
cleanup interval. Ctrl+C cancels the cleanup loop and stops the UDP server
without relying on `Console.ReadLine()` in noninteractive deployments.

No MainServer query, database lookup, or project-level runtime callback is
added to the relay.

### Logging

Accepted traffic logs subtype, matching ID, sender machine/entry, source, and
routing disposition at debug level. Admission, leave, capacity rejection, and
room expiry log at information or warning level. Malformed traffic logs a
bounded rejection reason without dumping profile/card payload bytes.

## Testing strategy

A dedicated `Tests/MultiplayerTests` xUnit project references `Application`,
`Infrastructure`, and `GCRelayServer`. Keeping these tests together exercises
the cross-process protocol contract while the production projects remain
independent.

### MainServer tests

- Initial admission returns event-scoped `99/10/89`, nonzero uint ID, entry 0.
- A retry returns the same room and entry without consuming capacity.
- Different events never share a room; conflicting card/machine identity is
  rejected.
- The second participant starts one 40-second formation deadline; later joins
  do not reset it; the fourth participant makes the room ready immediately.
- A single participant becomes ready at the 99-second search deadline.
- Uneven or concurrent polling does not change elapsed-time countdown speed.
- `8961` accepts native action zero, validates membership and event, and changes
  only the caller's message ID.
- `8709` parses all fields, associates the correct participant, and handles
  identical/conflicting retries.
- Cleanup uses last activity/terminal retention and protects recently removed
  IDs from reuse.
- XML response records preserve native field names and status/timing values.

### Relay tests

- Byte fixtures round-trip every header field without reflection or host
  endianness assumptions.
- Truncated, oversized, wrong-channel, mismatched-length, unknown, invalid-mode,
  invalid-payload, and inbound-response datagrams are rejected.
- First `A6` registers and becomes `A2` only for existing peers; subsequent
  `A6` reannouncements stay `A6` and do not loop.
- Lost-`A6` `A0` fallback registers, exchanges `A2`, and emits one `A1`.
- `B0` emits one `B1` without creating a room.
- `A3` removes without forwarding; `A7` is host-only and broadcasts.
- Broadcast excludes sender; concrete target reaches only that machine.
- Endpoint, entry, machine, profile identity, role, and fifth-member spoofing
  are rejected without corrupting the room.
- Manual time advancement expires rooms through the scheduled cleanup path.

Tests use a small manual `TimeProvider`; no test sleeps or wall-clock races are
required.

## Migration and compatibility

- Card request type numbers, controller routes, and certification relay fields
  remain unchanged. The redesign regression that shortened known 8709 ordinal
  XML names is corrected back to the pre-redesign/native names.
- Existing `Relay:RelayServer` and `Relay:RelayPort` configuration remains
  valid.
- Matching state is already transient, so no database migration is required.
- GCRelayServer's default UDP port remains 3333.
- The obsolete `BinarySerializer` package reference and central version entry
  are removed after the explicit codec is covered by fixtures.

## Verification boundary

Completion requires restore, Release build, and the full solution test suite,
plus focused multiplayer tests. That proves static layout, XML behavior,
elapsed-time state transitions, and pure routing decisions.

It does not prove real-cabinet acceptance or official-service timing parity.
Those remain explicit follow-up checks using packet captures and two- through
four-cabinet sessions.
