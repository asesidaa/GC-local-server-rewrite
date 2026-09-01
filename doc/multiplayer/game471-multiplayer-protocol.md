# game471 Multiplayer Protocol Reference

This document is the implementation-facing protocol reference for Groove
Coaster `game471.exe`. It covers the card-service matching lifecycle, native
multiplayer class ownership, UDP discovery/direct/public-relay transports, and
every routed message built or consumed by the game.

The address-level audit trail is retained in
[`game471-multiplayer-research.md`](game471-multiplayer-research.md). That
ledger records functions, vtables, object offsets, exported pseudocode, and the
IDA database provenance behind this reference.

## Evidence and authority

The protocol statements below use five evidence levels. Keeping these separate
is essential because the client binary can prove what a cabinet emits and what
it accepts, but it cannot reveal the retired relay or matching-service
implementation:

- **Native emitted**: constructed by a game471 client builder or serializer.
- **Native received/required**: checked or consumed by the game471 receive/state
  path.
- **Observed**: supplied from recordings or operator experience, but not
  recoverable from the client binary alone.
- **Pre-redesign implementation**: behavior or schema present in this repository
  before the 2026-08-25 redesign.
- **Server policy/inference**: a compatible choice made by this implementation,
  including relay behavior reconstructed from the client's needs rather than
  recovered from the official server.

The focused correction audit and claim-by-claim evidence matrix are in
[`game471-multiplayer-reaudit.md`](game471-multiplayer-reaudit.md).

The audited IDB's post-persistence SHA-256 is
`719F42E1DC4D5745F74774D66909D7C704FB93685CFE569E1F6E570E6578D4CE`.

## System boundary and native ownership

The native implementation is not one `CMatchingTask` class. Its responsibilities
are split across UI, HTTP/card-service models, and lower networking objects:

| Native owner | Responsibility |
| --- | --- |
| `CMatchingTask` | Matching UI/scene lifecycle. It does not own packet parsing or relay routing. |
| `CNesysOnlineMatching` and related `COnlineMatching*` models | Card-service entry, update, and result XML requests/responses. |
| Process-global `OnlineBattleNetwork` facade | Starts and coordinates discovery and battle-session networking. |
| UDP/60100 peer-discovery owner | LAN identity discovery, collision checks, goodbye, and expiry. |
| UDP/60200 session owner | Participant state, endpoints, HTTP polling, routed-message dispatch, and battle lifecycle. |
| UDP/60200 client embedded in the session owner | Datagram construction, parsing, send queues, and retransmission state. |

The corresponding server projects remain separate processes:

```text
game471.exe
   | card XML: certify, 8705, 8961, 8709
   v
MainServer  ---------------- advertises ----------------> GCRelayServer endpoint
   |                                                         ^
   | HTTP roster: event, matching_id, stable entry_no        | UDP channel 6
   +---------------------------------------------------------+ reconstructed membership
```

There is intentionally no runtime in-process state sharing between MainServer
and GCRelayServer. The shared contract is the `matching_id`, stable `entry_no`,
machine identity, maximum room size, and the relay endpoint advertised during
certification.

## Card-service matching protocol

### Relay endpoint handoff

The certification response supplies `relay_addr` and `relay_port`. The game
stores them independently from the online-matching response and copies them
into the public-relay endpoint when creating a session. Missing certification
data produces `0.0.0.0:0`.

A valid relay heartbeat response (`B1`) refreshes only the endpoint's source
IPv4 address and relay health tick; the client retains the port obtained from
certification.

### Request selectors and request data

| Native selector | Card request type | Operation |
| ---: | ---: | --- |
| `1901` | `8705` | Enter online matching. |
| `1902` | `8961` | Poll/update online matching. |
| `1903` | `8709` | Upload online battle result. |

Request `8705` serializes:

```text
machine_id, event_id, player_name, avatar_id, title_id, class_id,
group_id, tenpo_id, tenpo_name, pref_id, pref, message_id
```

Request `8961` serializes:

```text
action, event_id, matching_id, message_id
```

`CNesysOnlineMatching` initializes its request-action field at object offset
`+0x90` to zero and passes that unchanged field to every selector-`1902`
request. No writer to that field exists in the audited class. A live one-player
run on 2026-09-02 confirms `action = 0` on all 18 observed `8961` polls. The
earlier entry-dependent `1`/`2` interpretation was incorrect.

Request `8709` serializes:

```text
event_id, matching_id, class_id, group_id, result_score, result_star,
result_rank,
music_id_1st, music_id_2nd, music_id_3rd,
difficulty_lv_1st, difficulty_lv_2nd, difficulty_lv_3rd,
item_id_1st, item_id_2nd, item_id_3rd
```

These ordinal suffixes were validated from the complete NUL-terminated bytes
and from each direct operand in `OnlineBattleResultXml_WriteRequest_8709` at
`0x00540910`; they are not guesses made from IDA's shortened autogenerated
labels. The pre-redesign `OnlineMatchingResultData` model already contained all
nine correct names. The shortened `_1`/`_2`/`_3` names were introduced by the
redesign and were a regression, not a newly discovered protocol variant.

### Matching response record

Each `/root/online_matching/record` contains:

| XML field | Native type/width |
| --- | --- |
| `card_id` | string buffer, 17 bytes |
| `player_name` | string buffer, 33 bytes |
| `machine_id` | 32-bit integer |
| `event_id` | 32-bit integer |
| `avatar_id` | 32-bit integer |
| `title_id` | 32-bit integer |
| `class_id` | 32-bit integer |
| `group_id` | 32-bit integer |
| `tenpo_id` | 32-bit integer |
| `tenpo_name` | string buffer, 31 bytes |
| `pref_id` | 32-bit integer |
| `pref` | string buffer, 33 bytes |
| `message_id` | 32-bit integer |
| `matching_id` | 32-bit integer |
| `entry_no` | 32-bit integer |
| `status` | 32-bit integer |
| `matching_timeout` | 32-bit integer |
| `matching_wait_time` | 32-bit integer |
| `matching_remaining_time` | 32-bit integer |
| `entry_start` | string buffer, 20 bytes |

The native HTTP bridge compacts the parsed 228-byte record into a 176-byte
runtime entry. Its final four words are, exactly, `status`,
`matching_timeout`, `matching_wait_time`, and `matching_remaining_time`.

### Client-controlled and server-controlled timing

Binary-confirmed behavior:

- `status = 3` is the authoritative transition out of HTTP matching.
- `matching_wait_time * 1000` is the delay before the next `8961` request.
- `matching_remaining_time` is response/UI data and is not the worker's local
  control clock.
- The public client has no fixed local HTTP matching deadline.
- The client reannounces its UDP `A6` participant profile immediately before
  each `8961` update.

Live MainServer evidence on 2026-09-02 matches that control model. One player
entered at `03:30:59.707`; the cabinet then issued 18 `8961` requests from
`03:31:09.707` through `03:33:59.699`, all with `action = 0` and approximately
ten seconds apart. Polling continued well beyond the configured 99-second
search window and ended only when the server process was stopped. The client
therefore does not convert the displayed/countdown value into a local timeout.

The redesigned server uses an elapsed-time one-player deadline. Its action
validation now accepts the native `action = 0`, so the first poll at or after
the configured search deadline returns `status = 3` and zero timing values. A
one-player client has no independent fallback deadline; if MainServer never
returns status `3`, it continues polling indefinitely.

The original service's scheduling algorithm is therefore not present in the
client binary. Observed official behavior often displays `99` while searching
and appears to grant approximately 40 additional seconds after another player
joins. Those values are not binary-confirmed service defaults.

This implementation uses configurable server policy with provisional defaults:

| Policy | Default | Rationale |
| --- | ---: | --- |
| Search window | 99 seconds | Matches the commonly observed search display. |
| Formation window | 40 seconds | Matches the observed additional window after a second participant joins. |
| Poll interval | 10 seconds | Compatible with the historical `99/10/89` response shape. |
| Full-room transition | Immediate | No reason to delay after all four stable entries exist. |

An official-service packet capture is required to replace these provisional
policy values with provenance-equivalent defaults.

## UDP common envelope

All integer fields in the common 12-byte outer header are big-endian.

| Wire offset | Size | Field | Rule |
| ---: | ---: | --- | --- |
| `0x00` | 2 | `header_size` | Exactly `12`. |
| `0x02` | 2 | `remaining_size` | Exact datagram length minus 12. |
| `0x04` | 2 | `channel` | `3`, `4`, `5`, or `6`. |
| `0x06` | 2 | channel parameter | Native UDP/60200 builders emit zero; discovery uses it. |
| `0x08` | 4 | reserved | Native audited builders emit zero. |

UDP/60200 parsing requires at least 12 bytes, `header_size == 12`, an exact
length match, and channel `4`, `5`, or `6`. A public relay accepts channel `6`
only. The native outer receiver does **not** validate the channel parameter or
reserved dword. GCRelayServer's zero check is a strict game471-emission
allowlist and hardening policy, not a native receive requirement.

## UDP/60100 channel-3 discovery

Discovery datagrams are exactly 80 bytes: a 12-byte header plus a 68-byte
body. This is a LAN-only subsystem and is outside GCRelayServer's scope.

| Wire offset | Size | Meaning |
| ---: | ---: | --- |
| `0x06` | 2 | Discovery port/interface parameter. |
| `0x0C` | 2 | Discovery subtype. |
| `0x0E` | 2 | Secondary value; collision request/reply uses `0`/`1`. |
| `0x10` | 2 | Inner size, exactly `68`. |
| `0x12` | 2 | Reserved. |
| `0x14` | 2 | Protocol/session discriminator, initialized to zero. |
| `0x16` | 2 | Reserved. |
| `0x18` | 2 | Game-version major. |
| `0x1A` | 2 | Game-version minor. |
| `0x1C` | 4 | Machine identifier used for self filtering. |
| `0x20` | 16 | Version string (`%d.%02d`). |
| `0x30` | 16 | Local MAC-address string. |
| `0x40` | 16 | Local IPv4-address string. |

| Subtype | Secondary | Operation |
| ---: | ---: | --- |
| `0x0000` | `0` request, `1` reply | Collision probe/acknowledgement. |
| `0x0001` | normally `0` | Identity publication/refresh. |
| `0x0002` | normally `0` | Identity request; peer replies with subtype `1`. |
| `0x0003` | normally `0` | Goodbye and identity removal. |
| `0x000E` | normally `0` | Unavailable/interface-change removal. |

Startup broadcasts subtype `2`. Identity maintenance runs every five seconds,
expiry processing every second, and peers expire after ten seconds. The native
table contains 16 entries.

## UDP/60200 transport channels

| Channel | Purpose |
| ---: | --- |
| `4` | Fixed-size direct/LAN setup requests and participant snapshots. |
| `5` | Direct peer-to-peer routed gameplay messages. |
| `6` | Public-relay routed gameplay and membership messages. |

GCRelayServer owns only channel `6`; it must not imitate channel-3 discovery or
channel-4/5 direct coordination.

### Channel-4 direct request

Every channel-4 request is exactly 1,204 bytes: a 12-byte outer header and a
1,192-byte body.

| Inner offset | Size | Meaning |
| ---: | ---: | --- |
| `0x00` | 2 | Request subtype. |
| `0x02` | 2 | Secondary value. |
| `0x04` | 2 | Body size, exactly `1192`. |
| `0x06` | 2 | Request value 1. |
| `0x08` | 2 | Request value 2. |
| `0x0A` | 2 | Participant count. |
| `0x0C` | 12 | Reserved. |
| `0x18` | 60 | Local machine/network identity. |
| `0x54` | 80 | Four endpoint records. |
| `0xA8` | 1,024 | Four 256-byte participant records. |

| Subtype | Meaning |
| ---: | --- |
| `0xE000` | Maps secondary value `0/1/2` to session outcomes `1/2/3`. |
| `0x0000` | Initial direct-peer election/probe. |
| `0x0001` | Direct address/setup reply and source-endpoint installation. |
| `0x0002` | Join/setup request. |
| `0x0003` | Cancel/leave/reset of the initial branch. |
| `0x0004` | Full-roster installation and ready transition. |
| `0x0005` | Roster/readiness poll and role-dependent branch. |
| `0x0006` | Coordinator/host snapshot handoff. |

The client compacts each 256-byte wire participant into 152 bytes locally;
implementations must retain the 256-byte wire stride.

### Channel-5/6 routed header

All header integers are big-endian. For these channels,
`remaining_size = 24 + payload_size` and total datagram length is
`36 + payload_size`.

| Wire offset | Size | Field |
| ---: | ---: | --- |
| `0x0C` | 2 | Subtype. |
| `0x0E` | 2 | Mode/status. |
| `0x10` | 2 | Payload byte count. |
| `0x12` | 2 | Route/member parameter. |
| `0x14` | 4 | Matching/session ID. |
| `0x18` | 4 | Sender entry number or related route key. |
| `0x1C` | 4 | Sender machine ID. |
| `0x20` | 4 | Target machine ID. |
| `0x24` | variable | Payload. |

Target `0` and `0xFFFFFFFF` mean broadcast. A concrete nonzero target is a
machine ID. Channel 5 rejects a local machine as sender; channel 6 accepts only
target zero, `0xFFFFFFFF`, or the local machine ID at the receiving cabinet.

### Complete routed-message inventory

The mode, payload size, encoding, and retry facts in this table are recovered
from native client emitters. `OnlineBattlePacket_DecodeServerOrRouted` dispatches
by subtype but does not independently enforce the listed mode or
subtype-specific payload length. GCRelayServer intentionally uses the emitted
forms as its accepted-client allowlist.

| Subtype | Mode | Payload bytes | Encoding | Meaning and native send behavior |
| ---: | ---: | ---: | --- | --- |
| `0x0010` | `4` | 4 | One BE word | Participant phase/barrier bitfield; three sends plus retry tracking. |
| `0x0030` | `1` or `2` | 12 | Three BE words | Music selection. Guest mode `2`; host authoritative echo mode `1`; three sends plus retry. |
| `0x0031` | `4` | 12 | Three BE words | Music-selection commit/follow-up; three sends plus retry. |
| `0x0032` | `4` | 8 | Two BE words | Difficulty and selected item; three sends plus retry. |
| `0x0040` | direct `0`; relay `1` or `2` | 24 or 84 | Native little-endian scalars | Live tune snapshot; variable by sender role. |
| `0x0050` | `4` | 88 | 22 BE words | Final/result participant record; three sends plus retry. |
| `0x0060` | `4` | 4 | One BE word | Preset in-game chat/message ID. |
| `0x0070` | `2` | 8 | Two zero words | Guest liveness every three seconds. |
| `0x0071` | `1` | 16 | Four BE words | Host status vector every three seconds. |
| `0x00A0` | `0` | 256 | Participant profile | Registration; one send per state-machine attempt. |
| `0x00A1` | `0` | 256 | Participant profile | Registration acknowledgement consumed by the game. |
| `0x00A2` | `4` | 256 | Participant profile | New-member notification consumed by the game. |
| `0x00A3` | `0` | 8 | Two zero words | Relay-consumed participant leave. |
| `0x00A6` | `4` | 256 | Participant profile | Participant announcement; one send. |
| `0x00A7` | `1` | 8 | Two zero words | Host start/advance; three sends plus retry. |
| `0x00B0` | `0` | 8 | Two zero words | Relay heartbeat request; one send. |
| `0x00B1` | `0` | 8 | Two zero words | Relay heartbeat response consumed by the game. |

For `0032`, difficulty is `0` simple, `1` normal, `2` hard, or `3` extra.
For `0040`, the guest payload contains state plus five local values (24 bytes),
while the host payload contains state plus five arrays of four values (84
bytes). These are raw native-memory values and therefore little-endian, unlike
the other numeric gameplay payloads.

The 22 words in `0050` map from native result-model indices in this exact
sender order:

```text
5, 19, 20, 10, 11, 12, four packed booleans,
17, 18, 21, 22, 23, 24, 4, 25, 16, 15, 14, 13
```

Some domain names for those native result-model indices remain opaque. The
wire mapping is closed; implementations should not invent semantic names.

### Participant profile payload

Messages `A0`, `A1`, `A2`, and `A6` carry a 256-byte profile.

| Payload offset | Size | Field |
| ---: | ---: | --- |
| `0x00` | 40 | Ten BE profile words through `event_rank`. |
| `0x28` | 4 | Event EP high word. |
| `0x2C` | 4 | Event EP low word. |
| `0x30` | 16 | `card_id`. |
| `0x40` | 32 | `player_name`. |
| `0x60` | 30 | `tenpo_name`. |
| `0x7E` | 2 | Padding. |
| `0x80` | 16 | `pref`. |
| `0x90` | 112 | Zero padding. |

The first ten words are:

```text
avatar_id, title_id, relay profile extension, event_id, class_id,
group_id, tenpo_id, pref_id, message_id, event_rank
```

HTTP establishes identity and base profile data. Routed profiles reconcile
only the relay extension, event rank, and event EP words; they are not a second
authoritative identity source.

## Public online lifecycle

The native public path is:

```text
HTTP 8705 entry
    -> install matching snapshot
    -> UDP A6 announce
    -> UDP A0 register
    -> receive valid A1 for local matching_id + entry_no
    -> HTTP 8961 polling (A6 reannouncement before each poll)
    -> status 3
    -> gameplay/control routing
    -> HTTP 8709 result
```

### Reconstructed relay membership and routing policy

The client binary proves the packet order and the receive transitions that a
compatible relay must satisfy. It does not contain the official relay's room
algorithm. The following is the current server policy inferred from those
client needs and covered by local routing tests; it must not be presented as a
decompilation of the retired server.

For each nonzero `matching_id`, the relay maintains at most four members keyed
by machine ID and stable entry number.

1. The first valid `A6` from an unseen participant registers that member.
2. Existing participants receive the new profile as `A2`.
3. Existing participants respond natively by reannouncing their own `A6`.
4. A later `A6` from a known participant is forwarded as `A6`; rewriting every
   announcement to `A2` would create an `A2`/`A6` loop.
5. `A0` receives one `A1` self-acknowledgement after membership exists.
6. Because `A6` is a single UDP send, an idempotent `A0` may reconstruct a
   missing member and perform the same `A2` exchange before acknowledging.
7. `A3` removes the sender and is consumed by the relay because the client has
   no routed `A3` receive branch.
8. `A7` is delivered to all other room members.
9. `B0` receives one `B1` from the relay and does not require room membership.
10. Other valid channel-6 messages route by target: `0` and `0xFFFFFFFF`
    broadcast to all members except the sender; a concrete target is delivered
    only to that machine.

One valid `A1` or `B1` is sufficient for the client state machine. Duplicate
acknowledgements are not required by the binary. Whether the official relay
duplicates them can be established only by a packet capture.

The current relay rejects malformed lengths, non-channel-6 traffic, unsupported
inbound response subtypes, invalid emitted subtype/mode/payload combinations,
spoofed sender identity, conflicting machine/entry membership, and a fifth
member. Same-event membership and unique card fingerprints are anti-confusion
server policy layered over the HTTP-installed roster; the native receiver does
not perform those checks. A capacity rejection never clears an existing room.

## Reliability and expiry facts

Binary-confirmed client behavior:

- The UDP/60200 receive buffer is 8,192 bytes.
- `0010`, `0030`, `0031`, `0032`, `0050`, and `A7` use three immediate sends
  plus 3.5-second retry tracking.
- `A6`, `A0`, and `B0` are single sends per state-machine attempt.
- `A3` uses a separate send queue.
- Guest `0070` and host `0071` run every three seconds; peer liveness becomes
  stale after ten seconds.
- `B0` runs every 30 seconds; relay health becomes stale after 65 seconds
  without `B1`.

The client's retry rules do not prescribe relay-side duplicate sends. Relay
room expiration is server policy; the current ten-minute idle lifetime is
retained as a configurable compatibility default and must be enforced by a
periodic cleanup path, not only when another datagram arrives.

## Cross-process compatibility invariants

MainServer and GCRelayServer must independently enforce the same protocol
invariants without sharing memory:

- `matching_id` is nonzero and fits an unsigned 32-bit UDP field.
- A matching ID is not reused while delayed HTTP or UDP traffic could still be
  accepted.
- Rooms are scoped by `event_id` on MainServer and by `matching_id` on the
  relay.
- Membership is capped at four.
- Entry numbers are stable and unique in `0..3`.
- A card/machine retry of `8705` is idempotent.
- Relay membership binds machine ID, entry number, endpoint, and matching ID;
  later packets cannot silently mutate that identity.
- Certification advertises a reachable relay address and a valid 16-bit port.

## Remaining external validation

Static analysis and automated tests can prove the layouts and local routing
contracts above. They cannot prove parity with the retired official service.
The remaining acceptance work is:

1. Capture timestamped official `8705`/`8961` responses to establish the exact
   search, formation, poll, and countdown field policy.
2. Capture official relay traffic to determine whether it intentionally
   duplicates `A1`/`B1` or applies additional admission rules.
3. Run two-, three-, and four-cabinet sessions through matching, selection,
   gameplay, result upload, participant leave, relay loss, and expiry.
