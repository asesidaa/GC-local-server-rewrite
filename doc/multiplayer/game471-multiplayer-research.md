# game471 Multiplayer Reverse-Engineering Ledger

This is the living address-level evidence ledger for the `game471.exe`
multiplayer audit. Names marked **persisted** were applied to
`H:\gc\game471.exe.i64` and the database was saved.

The 2026-09-02 regression and evidence-boundary recheck is summarized in
[`game471-multiplayer-reaudit.md`](game471-multiplayer-reaudit.md).

## Provenance

- Database: `H:\gc\game471.exe.i64`
- Pre-annotation SHA-256: `3F911E373D18F4C3F11DACF5759AB7FF08847A4F365E8C0ED17B2896E7C47163`
- Post-persistence SHA-256: `719F42E1DC4D5745F74774D66909D7C704FB93685CFE569E1F6E570E6578D4CE`
- Post-persistence size/mtime: `105711519` bytes, `2026-09-01 20:43:25 UTC`
- Analysis backend: persistent IDA-CLI daemon using IDALib
- Raw exports: `H:\gc\runs\20260823T161327Z-8526efb2\multiplayer`
- Additional IDA-CLI artifacts: `H:\gc\runs\20260823T161327Z-8526efb2\artifacts\multiplayer`
- 2026-09-02 correction artifact:
  `H:\IDACLI\runs\20260831T042308Z-bfbb2f6a\artifacts\multiplayer-reaudit-evidence.json`
  (SHA-256 `938786DB01BC68B93B2038F7E8935DE969D12E94F55B16EC12F5866C9498F016`)
- Persistence batches: 55 + 42 + 9 function-rename operations and 12 + 12 + 8
  repeatable-comment operations, independently verified and saved to the IDB
  on 2026-08-25. The second batch corrects two earlier provisional names:
  `0x0055EDB0` is a session-lifecycle callback, while decoded packets are
  dispatched at `0x00563D10`; `0x00564C20` starts the worker whose thread
  procedure is `0x00564C00`.
- The 2026-09-02 correction batch added 39 function/accessor/string names,
  corrected six repeatable comments and one Hex-Rays pseudocode comment, and
  persisted complete 8709 string bytes/xrefs, action-zero control flow, native
  receiver boundaries, and the closed 0050 sender mapping.

## Confirmed native ownership

### Game task and HTTP model classes

| Class | Evidence | Confirmed role |
| --- | --- | --- |
| `CMatchingTask` | primary vftable `0x006FAB28`; secondary vftables `0x006FAB54` and `0x006FAB5C` | Matching scene/task. Owns UI/task lifecycle and enters the lower online-battle subsystem. |
| `COnlineMatchingController` | vftable `0x006CB83C` | Controller for `/root/online_matching` HTTP data. |
| `COnlineBattleResultController` | vftable `0x006CB964` | Controller for `/root/online_battle_result`. |
| `COnlineMatching` | vftable `0x006CC228` | One matching record model. |
| `COnlineMatchings` | vftable `0x006CC2A8` | Collection/owner for matching records. |
| `COnlineBattleResult` | vftable `0x006CC240` | Online battle result model. |
| `CNesysOnlineMatching` | vftable `0x006FB060` | HTTP matching bridge used by the online-battle state machine. |

`CMatchingTask` has three vptrs at object offsets `0x00`, `0x28`, and `0x48`. Its primary vtable contains initialization, update, start, cleanup, readiness, destruction, base teardown, and task-configuration slots. Confirmed persisted method names include:

- `CMatchingTask_Construct` at `0x005B82A0`
- `CMatchingTask_Initialize` at `0x005B82D0`
- `CMatchingTask_Update` at `0x005B8410`
- `CMatchingTask_Start` at `0x005B8270`
- `CMatchingTask_Cleanup` at `0x005B92F0`
- `CMatchingTask_OnMovieClipEvent` at `0x005B9200`
- `CMatchingTask_GetConfigPath` at `0x00444B80`

`COnlineMatchingController` is the XML serializer/parser owner, not the live
socket owner. It builds online-entry and matching-update request payloads and
parses `/root/online_matching/record` elements into `COnlineMatching` objects.
`COnlineMatchings` owns the resulting record collection. `CNesysOnlineMatching`
then copies those records into its thread-safe 176-byte-per-entry polling buffer
for consumption by the online-battle session worker.

### Lower multiplayer subsystem

The lower subsystem is not one UDP relay class. It contains two separate UDP transports plus an HTTP-aware session state machine:

| Component | Native root | Socket | Confirmed responsibility |
| --- | --- | --- | --- |
| Network facade | `OnlineBattleNetwork_Construct` `0x0055ECF0`; global getter `GetOnlineBattleNetwork` `0x0063BB10` | owns pointers to both lower components | Twelve-byte process-global facade. Starts and coordinates discovery/session ownership and exposes the participant API used by game tasks. |
| Peer discovery | `OnlineBattlePeerDiscovery_Initialize` `0x00566990` | UDP `60100` | Tracks local-interface changes and peer discovery with fixed 80-byte channel-3 packets. |
| Routed battle client | `OnlineBattleRelayClient_Construct` `0x0055C3C0` | UDP `60200` | Sends and receives variable-size channel-4/5/6 online-battle packets, including resend queues. |
| Session owner | `OnlineBattleSession_Construct` `0x00563BB0` | owns the UDP/60200 client at object offset `+1572` | Stores participants and endpoints, dispatches decoded packets, polls HTTP matching, and runs the multiplayer lifecycle. |
| Session state machine | `OnlineBattleSession_RunStateMachine` `0x00563D60` | worker loop | Coordinates HTTP matching, UDP setup, participant exchange, periodic messages, timeouts, retries, and failure states. |

The two UDP transports must remain distinct in documentation and implementation. UDP/60100 is not the public UDP/60200 routed battle channel.

`OnlineBattleNetwork_Initialize` is invoked during the game's global startup,
not by `CMatchingTask` construction. It allocates the 188-byte discovery object
and the 1,968-byte session object, publishes version and machine-identity data,
starts UDP/60100 discovery, waits until a machine identifier is available, and
then starts the session dispatch thread. `CMatchingTask` drives the user-facing
matching flow over this already-running subsystem.

## HTTP matching and relay-endpoint handoff

The card-service certification data is the source of the public relay endpoint.
`NesysCertifyResponse_ParseRelayEndpoint` at `0x0058CF70` reads `card_id`,
`relay_addr`, and `relay_port`. `CNesysCommTask_StoreCertifyResponse` at
`0x005917B0` retains a 128-byte relay-address buffer and a 16-bit relay port.
`OnlineBattleSession_LoadCertifiedRelayEndpoint` at `0x00561350` copies that
address and port into the session's 18-byte public-relay endpoint at object
offset `+1932`. Missing certification data becomes `0.0.0.0:0`. This endpoint
is not supplied by the `/root/online_matching` response.

The session keeps three endpoint families separate:

- `session + 52`: the selected direct coordinator endpoint;
- `session + 264`: four per-participant endpoint records;
- `session + 1932`: the certified public relay endpoint;
- `session + 1950`: the LAN subnet-broadcast endpoint on UDP `60200`.

A valid `B1` does not replace the certified endpoint wholesale.
`OnlineBattleSession_HandleHeartbeatResponse_B1` copies only the reply source
IPv4 string into `session + 1932`, retains the certified port, records the
receive tick, and marks relay health status `1`.

`CNesysOnlineMatching` uses these internal request selectors:

| Internal selector | Card request type | Native operation |
| ---: | ---: | --- |
| `1901` | `8705` | `OnlineBattleEntryAsync` |
| `1902` | `8961` | `OnlineBattleMatchingAsync` |
| `1903` | `8709` | `OnlineBattleResultAsync` |

The online-entry request (`8705`) serializes the following `data` fields:

- `machine_id`
- `event_id`
- `player_name`
- `avatar_id`
- `title_id`
- `class_id`
- `group_id`
- `tenpo_id`
- `tenpo_name`
- `pref_id`
- `pref`
- `message_id`

The matching-update request (`8961`) serializes `action`, `event_id`,
`matching_id`, and `message_id`. The battle-result request (`8709`) serializes
`event_id`, `matching_id`, `class_id`, `group_id`, `result_score`,
`result_star`, `result_rank`, `music_id_1st`, `music_id_2nd`,
`music_id_3rd`, `difficulty_lv_1st`, `difficulty_lv_2nd`,
`difficulty_lv_3rd`, `item_id_1st`, `item_id_2nd`, and `item_id_3rd`.

The suffixes above were revalidated from full NUL-terminated bytes, not IDA's
sometimes-truncated autogenerated label. Each literal has one direct code xref
inside `OnlineBattleResultXml_WriteRequest_8709` at `0x00540910`:

| Literal EA | Exact bytes before NUL | Serializer xref |
| ---: | --- | ---: |
| `0x006CBA28` | `music_id_1st` | `0x00540B96` |
| `0x006CBA18` | `music_id_2nd` | `0x00540BBE` |
| `0x006CBA08` | `music_id_3rd` | `0x00540BE6` |
| `0x006CB9F4` | `difficulty_lv_1st` | `0x00540C0E` |
| `0x006CB9E0` | `difficulty_lv_2nd` | `0x00540C36` |
| `0x006CB9CC` | `difficulty_lv_3rd` | `0x00540C5E` |
| `0x006CB9C0` | `item_id_1st` | `0x00540C86` |
| `0x006CB9B4` | `item_id_2nd` | `0x00540CAE` |
| `0x006CB9A8` | `item_id_3rd` | `0x00540CD2` |

The `CNesysOnlineMatching` object initializes the `action` source word at
offset `+0x90` to zero. Its worker reads that word unchanged when calling
`NesysCard_RequestOnlineMatchingUpdate_8961`; the audited class contains no
later writer. MainServer runtime logging on 2026-09-02 independently confirms
`action = 0` for every observed one-player update request.

Each `/root/online_matching/record` response element contains:

| `COnlineMatching` offset | XML field | Native representation |
| ---: | --- | --- |
| `0x18` | `card_id` | 17-byte string buffer |
| `0x29` | `player_name` | 33-byte string buffer |
| `0x4C` | `machine_id` | 32-bit integer |
| `0x50` | `event_id` | 32-bit integer |
| `0x54` | `avatar_id` | 32-bit integer |
| `0x58` | `title_id` | 32-bit integer |
| `0x5C` | `class_id` | 32-bit integer |
| `0x60` | `group_id` | 32-bit integer |
| `0x64` | `tenpo_id` | 32-bit integer |
| `0x68` | `tenpo_name` | 31-byte string buffer |
| `0x88` | `pref_id` | 32-bit integer |
| `0x8C` | `pref` | 33-byte string buffer |
| `0xB0` | `message_id` | 32-bit integer |
| `0xB8` | `matching_id` | 32-bit integer |
| `0xBC` | `entry_no` | 32-bit integer |
| `0xC0` | `status` | 32-bit integer |
| `0xC4` | `matching_timeout` | 32-bit integer |
| `0xC8` | `matching_wait_time` | 32-bit integer |
| `0xCC` | `matching_remaining_time` | 32-bit integer |
| `0xD0` | `entry_start` | 20-byte string buffer |

`CNesysOnlineMatching` deliberately compacts a response record from 228 bytes
to a 176-byte polling entry. The compact entry retains the identity strings and
all numeric fields above except `entry_start`; this is a native client model
layout, not a UDP wire layout.

The compact record's final words are exact: word `40` is `status`, word `41` is
`matching_timeout`, word `42` is `matching_wait_time`, and word `43` is
`matching_remaining_time`. When installing a response into the live session,
the client copies status, timeout, and wait time. In state `0x80000` it:

- exits HTTP polling when status is `3`;
- waits `matching_wait_time * 1000` milliseconds between update requests;
- sends update `action = 0` from the unchanged `CNesysOnlineMatching +0x90`
  field;
- reannounces `A6` immediately before each selector-`1902` update request.

The native worker does not use `matching_remaining_time` as its control clock;
it remains response/UI data. For deterministic server behavior, countdown data
should be derived from elapsed time rather than the number or order of client
polls.

The public-online client has no fixed local HTTP-matching deadline. Static
binary evidence proves that the service controls progress through `status` and
controls the next poll through `matching_wait_time`. The binary cannot reveal
the original service's room-scheduling algorithm.

Live evidence closes the client-timeout question. On 2026-09-02 one player
entered matching at `03:30:59.707`, then sent 18 updates at roughly ten-second
intervals from `03:31:09.707` through `03:33:59.699`. Every update carried
`action = 0`; polling continued past 99 seconds and stopped only with the
MainServer process. This exposed the redesigned `1`/`2` action-validation
regression. That validation now accepts action zero, allowing the elapsed-time
room aggregate to return `status = 3` on the first poll at or after the server
deadline. Cabinet acceptance of the corrected build remains separate from this
static/test proof.

Operator/video observation supplied on 2026-08-25: the UI often appears to show
`99` while searching; official timing varies, and after another participant
matches the service appears to grant roughly another 40 seconds. The binary
also contains an explicit `99` display/sentinel branch and a separate
90-second *local/direct* countdown, neither of which proves an HTTP default.
The clean server model is therefore adaptive and configurable:

- an event-scoped searching phase controlled by MainServer;
- a formation deadline started when the second unique participant joins;
- a provisional default formation window of 40 seconds;
- response `matching_remaining_time` derived from that deadline;
- response `matching_wait_time` used only as the client poll interval;
- `status = 3` used as the authoritative transition out of HTTP matching.

Exact official search/formation durations require an official-service packet
capture (or equivalent timestamped response evidence); they cannot be recovered
from `game471.exe` alone. The pre-redesign server's `20/5/5` values were
historical implementation behavior. The redesigned `99`-second search,
`40`-second formation, and `10`-second poll defaults are explicit provisional
server policy informed by observation, not binary-derived service constants.

### Current MainServer implementation and historical baseline

MainServer and GCRelayServer remain justified as separate processes. Their
current server-side responsibilities are:

| Project/component | Current ownership |
| --- | --- |
| `Application.Game.Server.CertifyCommandHandler` | advertises configured `relay_addr` and `relay_port` |
| `CardController` | maps card request types `8705`, `8961`, and `8709` to native Mediator commands |
| `StartOnlineMatchingCommandHandler` | parses the exact 8705 entry request and joins the event-scoped in-memory room |
| `OnlineMatchingService` / `OnlineMatchRoom` | owns nonzero uint matching IDs, stable entries, elapsed-time phases, membership, transient results, and cleanup indexes |
| `UpdateOnlineMatchingCommandHandler` | parses native action zero, validates card/event/match membership, updates only the caller's `message_id`, and returns the current roster |
| `UploadOnlineMatchingResultCommandHandler` | parses all exact 8709 fields, validates and associates the result, and returns status `1` on success |
| `CleanupOnlineMatchesJob` | invokes HTTP-room cleanup; the service applies idle/terminal retention and matching-ID tombstones |
| `GCRelayServer` | independently reconstructs transient UDP membership and routes channel `6` |

The separation is compatible with the native protocol: HTTP is authoritative
for roster allocation and base participant profiles, while UDP reconstructs
transport membership from `A6/A0` using the shared `matching_id`. No in-process
state sharing or project merge is required. The cross-project contract is:

- certification must advertise an address and 16-bit port reachable by every
  cabinet;
- HTTP matching IDs must be nonzero unsigned 32-bit values because that is the
  UDP field width;
- HTTP rooms and UDP rooms both cap membership at four;
- HTTP entry numbers remain stable in `0..3` and pair with machine IDs;
- the HTTP event/roster boundary and UDP matching ID must never be reused while
  either side can still receive delayed packets.

The pre-redesign repository already contained
`OnlineMatchingResultData` with all nine correct ordinal XML names and
`OnlineMatchingUpdateData` with the `action` field. Those definitions were
useful evidence and should have been preserved. They did not mean the old
pipeline consumed the data completely: the old 8961 handler deserialized a
different entry DTO and ignored action semantics, while the old 8709 handler
returned status `1` without parsing its correct result-data model.

The redesign fixed the old room, identity, elapsed-time, result-association,
cleanup, codec, and relay-ownership problems, but commit `2e76ff4` shortened the
nine known 8709 field names and introduced entry-dependent action `1`/`2`.
Both changes were redesign regressions. The current implementation restores the
known ordinal names and accepts only the native action value zero, with focused
deserialization, handler, and service tests.

## Common UDP outer header

`OnlineBattlePacket_WriteOuterHeader` at `0x0055D570` writes the common 12-byte envelope. All integer fields are big-endian on the wire.

| Wire offset | Size | Field | Confirmed rule |
| ---: | ---: | --- | --- |
| `0x00` | 2 | `header_size` | Always `12`; this is not a magic value. |
| `0x02` | 2 | `remaining_size` | Datagram length minus 12. |
| `0x04` | 2 | `channel` | `3`, `4`, `5`, or `6` as described below. |
| `0x06` | 2 | channel parameter/reserved | Native UDP/60200 builders emit zero; peer discovery uses it. |
| `0x08` | 4 | reserved | Native audited builders emit zero. |

`OnlineBattlePacket_ParseDatagram` at `0x0055D770` validates UDP/60200 input as follows:

1. Datagram size is at least 12 bytes.
2. `header_size == 12`.
3. `header_size + remaining_size` equals the exact received datagram length.
4. Only channels `4`, `5`, and `6` enter the routed battle decoder.

It does not validate the outer channel parameter or reserved dword. Requiring
those emitted-zero values in GCRelayServer is a strict game471-client allowlist
and hardening decision, not a client receive check.

## UDP/60100 peer-discovery protocol

`OnlineBattlePeerDiscovery_HandleDatagram` at `0x005665A0` requires an exact 80-byte datagram with `header_size = 12`, `remaining_size = 68`, and `channel = 3`.

Confirmed inner fields are:

| Wire offset | Size | Native meaning |
| ---: | ---: | --- |
| `0x06` | 2 | discovery port/interface parameter |
| `0x0C` | 2 | discovery subtype |
| `0x0E` | 2 | secondary subtype parameter; collision request/reply uses `0`/`1` |
| `0x10` | 2 | inner size, `68` |
| `0x12` | 2 | reserved |
| `0x14` | 2 | protocol/session discriminator, initialized as `0` |
| `0x16` | 2 | reserved |
| `0x18` | 2 | game-version major |
| `0x1A` | 2 | game-version minor |
| `0x1C` | 4 | machine identifier used for self filtering |
| `0x20` | 16 | formatted version string, `"%d.%02d"` |
| `0x30` | 16 | local MAC-address string |
| `0x40` | 16 | local IPv4-address string |

The full channel-3 subtype set and effects are:

| Subtype | Secondary | Native operation |
| ---: | ---: | --- |
| `0x0000` | `0` request, `1` reply | collision probe/acknowledgement |
| `0x0001` | normally `0` | identity publication or refresh |
| `0x0002` | normally `0` | identity request; receiver answers directly with subtype `1` |
| `0x0003` | normally `0` | explicit goodbye; removes the identity |
| `0x000E` | normally `0` | unavailable/interface-change notice; removes the identity |

Startup broadcasts subtype `2` to solicit peers. Identity/unavailable maintenance
runs every five seconds, expiry processing runs once per second, and a peer entry
expires ten seconds after its last accepted publication. The native table has
16 entries, each 72 bytes, holding active/expiry/compatibility state plus the
identity fields above.

## UDP/60200 routed battle protocol

The UDP/60200 client uses three channels:

| Channel | Native decode path | Confirmed role |
| ---: | --- | --- |
| `4` | `OnlineBattlePacket_DecodeClientRequest` | Fixed-size direct/LAN setup requests and participant snapshots. |
| `5` | `OnlineBattlePacket_DecodeServerOrRouted` | Direct peer-to-peer gameplay messages. |
| `6` | `OnlineBattlePacket_DecodeServerOrRouted` | Public-relay routed gameplay and matching messages. |

Gameplay builders select channel `5` for the direct/LAN path and channel `6`
for the online/public-relay path. In the latter path the destination is the
certified relay endpoint and the target field is normally zero. A central
`GCRelayServer` therefore owns channel `6` behavior only; it must not simulate
the separate UDP/60100 discovery protocol or direct channel-4/5 coordination.

### Channel 5/6 common inner header

The pre-redesign `GCRelayServer.RelayPacket` began at the correct offsets but
used several placeholder names. The confirmed wire layout is:

| Wire offset | Size | Pre-redesign C# field | Native meaning |
| ---: | ---: | --- | --- |
| `0x0C` | 2 | `RequestSubType` | message subtype |
| `0x0E` | 2 | `Unknown1` | subtype mode/status |
| `0x10` | 2 | `DataSize` | payload byte count |
| `0x12` | 2 | `Unknown2` | route/member parameter |
| `0x14` | 4 | `MatchingId` | matching/session identifier |
| `0x18` | 4 | `EntryNo` | sender entry number or related route key |
| `0x1C` | 4 | `MachineId` | sender machine identifier; native channel-5 validation rejects self-sent packets |
| `0x20` | 4 | `Unknown3` | target machine identifier; channel-6 validation accepts zero, `0xFFFFFFFF`, or this machine |
| `0x24` | variable | `Data` | subtype-specific payload |

For these channels, `remaining_size = 24 + payload_size` and total datagram size is `36 + payload_size`.

Channel `5` rejects packets whose sender machine ID equals the local machine.
Channel `6` accepts only target machine ID `0`, `0xFFFFFFFF`, or the local
machine ID. Neither path safely cross-checks every decoded inner size against
the remaining datagram before all allocation/copy operations, so the server
must enforce stricter exact-length and subtype-size validation than the client.

### Channel 4 request layout

All channel-4 builders allocate 1,204-byte datagrams: a 12-byte outer header
plus a 1,192-byte request body. The body contains:

| Inner offset | Size | Native meaning |
| ---: | ---: | --- |
| `0x00` | 2 | request subtype |
| `0x02` | 2 | secondary value |
| `0x04` | 2 | body size, `1192` |
| `0x06` | 2 | request value 1 |
| `0x08` | 2 | request value 2 |
| `0x0A` | 2 | participant count |
| `0x0C` | 12 | reserved |
| `0x18` | 60 | local machine/network identity |
| `0x54` | 80 | four endpoint records: machine ID plus IPv4 string[16] |
| `0xA8` | 1,024 | four 256-byte participant wire records |

The local decoded representation compacts each participant into 152 bytes. The wire stride must not be replaced with the compact in-memory stride.

Channel-4 validation additionally rejects the local machine ID as sender and
requires the identity discriminator plus both version words to equal the local
values. These are direct-peer trust checks, distinct from relay membership.

## Complete native message inventory

### Channel-4 request subtypes

| Subtype | Native builder | Total bytes | Confirmed handling |
| ---: | --- | ---: | --- |
| `0xE000` | `OnlineBattlePacket_BuildRequest_E000` | 1,204 | maps secondary value `0/1/2` to session outcomes `1/2/3` |
| `0x0000` | `OnlineBattlePacket_BuildRequest_0000` | 1,204 | initial direct-peer election/probe |
| `0x0001` | `OnlineBattlePacket_BuildRequest_0001` | 1,204 | direct address/setup reply; records the source endpoint |
| `0x0002` | `OnlineBattlePacket_BuildRequest_0002` | 1,204 | join/setup request |
| `0x0003` | `OnlineBattlePacket_BuildRequest_0003` | 1,204 | cancel/leave/reset of the initial branch |
| `0x0004` | `OnlineBattlePacket_BuildRequest_0004` | 1,204 | installs a full roster and advances to ready |
| `0x0005` | `OnlineBattlePacket_BuildRequest_0005` | 1,204 | roster/readiness poll and role-dependent branch |
| `0x0006` | `OnlineBattlePacket_BuildRequest_0006` | 1,204 | coordinator/host snapshot handoff |

### Channel-5/6 routed subtypes built by the game

| Subtype | Mode | Payload | Encoding and native send behavior |
| ---: | ---: | ---: | --- |
| `0x0010` | `4` | 4 | one big-endian bitfield; three sends and slot-0 retry tracking |
| `0x0030` | `1` or `2` | 12 | three big-endian words; three sends and 3.5-second retry tracking |
| `0x0031` | `4` | 12 | three big-endian words; three sends and slot-1 retry tracking |
| `0x0032` | `4` | 8 | two big-endian words; three sends and slot-2 retry tracking |
| `0x0040` | direct `0`; relay `1` or `2` | variable | raw native-memory payload, therefore little-endian scalar fields |
| `0x0050` | `4` | 88 | 22 big-endian words; three sends and slot-3 retry tracking |
| `0x0060` | `4` | 4 | one big-endian preset-message ID |
| `0x0070` | `2` | 8 | two zero words; guest sends every three seconds |
| `0x0071` | `1` | 16 | four big-endian status words; host sends every three seconds |
| `0x00A0` | `0` | 256 | participant/profile registration; single send per attempt |
| `0x00A3` | `0` | 8 | two zero words; special queued leave path |
| `0x00A6` | `4` | 256 | participant/profile announcement; single send |
| `0x00A7` | `1` | 8 | two zero words; three sends plus retry tracking |
| `0x00B0` | `0` | 8 | two zero words; single heartbeat request |

The receive state machine additionally handles `A1`, `A2`, and `B1`. These
are relay/peer-side counterparts without a game-side builder in this path.

### Participant/profile payload

The local compact participant profile is 152 bytes:

| Local offset | Field |
| ---: | --- |
| `0x00` | `avatar_id` |
| `0x04` | `title_id` |
| `0x08` | reserved/relay profile extension |
| `0x0C` | `event_id` |
| `0x10` | `class_id` |
| `0x14` | `group_id` |
| `0x18` | `tenpo_id` |
| `0x1C` | `pref_id` |
| `0x20` | `message_id` |
| `0x24` | `event_rank` |
| `0x28` | event EP low word |
| `0x2C` | event EP high word |
| `0x30` | reserved/padding |
| `0x34` | `card_id[16]` |
| `0x44` | `player_name[32]` |
| `0x64` | `tenpo_name[32]` |
| `0x84` | `pref[16]` |

`A0`, `A1`, `A2`, and `A6` declare a 256-byte wire payload:

| Payload offset | Size | Wire field |
| ---: | ---: | --- |
| `0x00` | 40 | ten big-endian profile words through `event_rank` |
| `0x28` | 4 | event EP high word |
| `0x2C` | 4 | event EP low word |
| `0x30` | 16 | `card_id` |
| `0x40` | 32 | `player_name` |
| `0x60` | 30 | `tenpo_name` |
| `0x7E` | 2 | padding |
| `0x80` | 16 | `pref` |
| `0x90` | 112 | zero padding |

`OnlineBattleSession_InstallHttpMatchingSnapshot` establishes identity and
base profile data from HTTP. `OnlineBattleSession_MergeRoutedParticipantProfile`
then changes only local offsets `0x08`, `0x24`, `0x28`, and `0x2C`. The relay
payload is therefore a dynamic extension/reconciliation layer, not the source
of authoritative player identity.

### Gameplay/control payload semantics

| Subtype | Confirmed producer/consumer semantics |
| ---: | --- |
| `0x0010` | Participant phase/barrier bitfield. Received values are ORed per participant. |
| `0x0030` | Music selection: word 0 is music ID, word 1 is the eligible-participant bitmask, and word 2 is a selection slot/index or `-1` sentinel. A guest sends mode `2`; the host receives it and broadcasts the authoritative mode-`1` echo. |
| `0x0031` | Three-word music-selection commit/follow-up record. |
| `0x0032` | Word 0 is difficulty (`0` simple, `1` normal, `2` hard, `3` extra); word 1 is selected item ID. |
| `0x0040` | Live tune snapshot. Host payload is 84 bytes: state then five arrays of four values (`float score[4]`, a second `float` gameplay score/gauge metric, two `int` judgement/effect arrays, and `uint` queue/countdown). Guest payload is the same state plus its five local values, 24 bytes. Consumers apply these values during tune state `18`. |
| `0x0050` | Final/result participant record, 22 words. Sender order from the 120-byte result model is indices `5,19,20,10,11,12`, four packed booleans, then `17,18,21,22,23,24,4,25,16,15,14,13`. The receiver reconstructs the inverse layout. Several model field domain names remain opaque, but the wire mapping is closed. |
| `0x0060` | Preset in-game chat/message ID emitted by `CPanel_Message`. |
| `0x0070` | Guest liveness. Payload is eight zero bytes; the host refreshes the sender slot's tick. |
| `0x0071` | Host status vector: `0` absent, `1` active, `2` stale. Guests apply it to peer status; active refreshes the tick and stale marks the slot stale. |
| `0x00A3` | Participant leave request. There is no native routed `A3` receive branch, so this is relay-consumed control. |
| `0x00A7` | Host start/advance notification. |
| `0x00B0` / `0x00B1` | Relay heartbeat request/response. Requests run every 30 seconds; relay health becomes stale after 65 seconds without `B1`. |

The high-level API-to-builder mapping, numeric subtype, payload size, byte
order, mode, native handler branch, and reliability behavior are closed for
every game-built message. Only a few opaque `0050` result-model field labels
remain intentionally unnamed; inventing labels would be less accurate than the
documented exact word mapping.

## Public online-matching state sequence and relay contract

The public path in `OnlineBattleSession_RunStateMachine` is ordered:

1. State `0x10000` starts/polls HTTP online entry selector `1901`.
2. A successful response installs compact matching records, enters
   `0x20000`, and sends `A6`.
3. The next transition enters `0x40000` and sends `A0`.
4. A valid `A1` matching the local `matching_id` and `entry_no` advances to
   `0x80000`.

The native `A2` handler merges the newly seen peer profile and then broadcasts
the local participant's `A6` again. The machine-ID parameter passed through the
high-level `A6` wrapper is not encoded as a specific target; `A6` uses target
zero. These client transitions constrain a compatible membership exchange, but
the following routing algorithm is reconstructed server policy, not code
recovered from the official relay:

1. First `A6` from an unseen participant joins membership and must be exposed
   to existing participants as `A2`.
2. Existing participants receive `A2` and reannounce themselves with `A6`.
3. A later `A6` from an already known participant is forwarded as `A6`.
   Rewriting every `A6` to `A2` would create an `A2`/`A6` loop.
4. `A0` receives an `A1` self-acknowledgement once membership exists.
5. `A7` is relayed to the other members.
6. `A3` removes the sender from membership and is consumed by the relay.
7. `B0` receives one `B1` self-acknowledgement.
8. Other channel-6 messages route by target machine ID. Target `0` and
   `0xFFFFFFFF` are broadcasts excluding the sender; a concrete target is
   delivered only to that machine.

Because `A6` is a single UDP send, a robust compatible relay may use the
profile-bearing `A0` as an idempotent membership fallback when the preceding
`A6` was lost, then perform the same new-member `A2` exchange before replying
with `A1`. This fallback is an implementation resilience choice derived from
the native ordering, not a separately observed server behavior.

The binary requires one valid `A1`/`B1` to advance. It does not establish that
the production relay duplicates replies. The pre-redesign relay's two
identical acknowledgements were folklore, not a native protocol requirement;
the redesigned relay sends one. An external packet capture is still required
to determine whether the original production relay intentionally duplicated
them.

## Native reliability and lifecycle facts

- UDP/60200 receive uses an 8,192-byte buffer and rejects malformed length/channel combinations before dispatch.
- `0010`, `0030`, `0031`, `0032`, `0050`, and `A7` use three immediate sends plus 3.5-second retry tracking. Separate resend storage exists for slots `1..3`, with a linked structure for slot `0`.
- `A6`, `A0`, and `B0` are single sends per state-machine attempt; `A3` uses its own queue.
- Guest `0070` and host `0071` run every three seconds. Peer liveness becomes stale after ten seconds.
- `B0` runs every 30 seconds; relay health becomes stale after 65 seconds without `B1`.
- Discovery identity maintenance runs every five seconds and peer expiry runs every one second with a ten-second deadline.
- Session states are bit-like numeric values including `0x10`, `0x20`, `0x40`, `0x100`, `0x200`, `0x400`, `0x1000`, `0x2000`, `0x10000`, `0x20000`, `0x40000`, and `0x80000`, plus terminal/error states. Friendly state names will be assigned only after all transitions are closed.
- `CNesysOnlineMatching` is polled from the same worker state machine during peer selection and retry branches; HTTP matching and UDP transport are one lifecycle, not independent features.

## Pre-redesign implementation comparison: confirmed historical issues

These were evidence-backed mismatches in the relay implementation audited
before the approved redesign. They are retained as history and must not be read
as a description of the current code:

1. `RelayPacket.Magic` is actually `header_size`, fixed at 12.
2. `RequestMainType` is the channel discriminator and must be validated/handled as channel `3`, `4`, `5`, or `6` depending on socket/protocol scope.
3. `Unknown3` in channel 5/6 is used as a target-machine filter by the native receiver.
4. The server models only five subtype constants, while the game contains the complete inventory above.
5. The server changes only the subtype when fabricating `A1`/`B1`; native validity also depends on channel and routing fields.
6. The server treats every unrecognized subtype as an opaque room broadcast, but native routing distinguishes target machine, sender machine, matching ID, entry number, and channel semantics.
7. Room membership is inferred only from source endpoint and `matching_id`; the game carries explicit machine and target identities.
8. The first meaningful online packet is `A6`, but the server creates rooms only
   on the later `A0`. A fresh `A6` is therefore dropped or has no membership
   context. Forwarding it unchanged also cannot teach existing members about a
   joining profile via the required `A2` exchange.
9. Duplicate `A1`/`B1` sends are not required by the client state machine.
10. `A3` is forwarded even though the client has no `A3` receive handler; the
    relay should consume it as membership removal.
11. A fifth member cleared the entire room. HTTP matching is capped at four
    participants; defensive relay behavior should reject/drop the new member
    without destroying the existing session.
12. `RoomManager.EvictExpiredRooms` was not scheduled. The ten-minute idle
    lifetime was implementation policy, not a duration recovered from the
    binary, and needed to be explicit/configurable.

The redesign separated the codec, native message model, routing policy,
room/session state, and socket host, with tests derived from native byte
fixtures. The 2026-09-02 re-audit found no native-emission incompatibility in
that relay implementation; its admission and exact-contract checks are now
explicitly documented as policy/hardening where the client receiver does not
perform the same validation.

## Raw evidence exports

- `CMatchingTask-decompiles.json`
- `udp-clusters-decompiles.json`
- `relay-message-builders.json`
- `packet-builders-and-layouts.json`
- `packet-codec-core.json`
- `relay-high-level-api.json`
- `relay-pipeline-state-machines.json`
- `relay-callbacks-and-discovery.json`
- `http-matching-classes.json`
- `H:\IDACLI\runs\20260831T042308Z-bfbb2f6a\artifacts\multiplayer-reaudit-evidence.json`
- `artifacts/multiplayer/http-pipeline-decompiles.json`

These artifacts are analysis evidence. The protocol reference and correction
audit restate the verified conclusions without requiring access to the IDA run
directory.
