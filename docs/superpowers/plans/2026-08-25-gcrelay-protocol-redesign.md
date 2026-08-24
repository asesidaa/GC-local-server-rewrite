# GCRelayServer Protocol Redesign Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace GCRelayServer's endpoint-only opaque broadcast loop with a byte-exact channel-6 codec, native-compatible membership state machine, identity-aware routing, and scheduled room expiry.

**Architecture:** An immutable `RelayDatagram` and explicit `BinaryPrimitives` codec own wire layout. A descriptor table owns message direction, mode, size, and role rules. Locked rooms bind matching ID, entry, machine, endpoint, and stable profile identity. A pure router returns outbound transmissions for `A6/A2`, `A0/A1`, `A3`, `A7`, `B0/B1`, and every gameplay subtype. NetCoreServer remains a thin UDP adapter; a cancellable periodic worker owns expiry.

**Tech Stack:** .NET 10, C# 14, `System.Buffers.Binary`, NetCoreServer 8.0.7, Serilog 4.4.0, xUnit v3 3.2.2.

## Global Constraints

- Treat `doc/multiplayer/game471-multiplayer-protocol.md` as the wire and lifecycle authority.
- Accept only UDP/60200 channel 6; do not implement channels 3, 4, or 5.
- Preserve GCRelayServer as a standalone executable and keep default UDP port 3333.
- Enforce the exact 12-byte outer header, 24-byte routed header, and subtype payload sizes.
- Treat `A1`, `A2`, and `B1` as relay-output-only messages.
- Send one acknowledgement; do not retain duplicate `A1`/`B1` folklore.
- Never clear a valid room when a fifth member or malformed packet arrives.
- Bind stable membership identity and exclude the sender from broadcasts.
- Retain a configurable ten-minute idle expiry and run cleanup independently of incoming traffic.
- Remove `BinarySerializer` only after independent byte-fixture tests pass.
- Execute after the MainServer plan, which creates `Tests/MultiplayerTests`.
- Use test-first red/green cycles and commit each task independently.

## File Map

- Modify `Tests/MultiplayerTests/MultiplayerTests.csproj`: reference GCRelayServer.
- Create `Tests/MultiplayerTests/Relay/RelayDatagramCodecTests.cs`: independent wire fixtures and rejection cases.
- Create `Tests/MultiplayerTests/Relay/RelayRoomRegistryTests.cs`: identity, capacity, activity, and expiry.
- Create `Tests/MultiplayerTests/Relay/RelayMembershipRouterTests.cs`: `A6/A2`, `A0/A1`, `A3`, `A7`, and `B0/B1`.
- Create `Tests/MultiplayerTests/Relay/RelayGameplayRouterTests.cs`: role, broadcast, unicast, and spoof checks.
- Create `Tests/MultiplayerTests/Relay/RelayServerOptionsTests.cs`: CLI compatibility and bounds.
- Delete `GCRelayServer/RelayPacket.cs`: reflection-based mutable wire model.
- Delete `GCRelayServer/RelayPacketTypes.cs`: incomplete constant set.
- Create `GCRelayServer/Protocol/RelayMessageType.cs`: complete routed subtype enum.
- Create `GCRelayServer/Protocol/RelayDatagram.cs`: immutable semantic packet.
- Create `GCRelayServer/Protocol/RelayDatagramCodec.cs`: strict big-endian parser/writer.
- Create `GCRelayServer/Protocol/RelayProtocol.cs`: descriptors and stateless validation.
- Create `GCRelayServer/Protocol/RelayRejection.cs`: bounded rejection codes/details.
- Delete `GCRelayServer/Room.cs`: endpoint-only room.
- Delete `GCRelayServer/RoomManager.cs`: opportunistic endpoint registry.
- Create `GCRelayServer/Routing/RelayProfileIdentity.cs`: stable event/card fingerprint parser.
- Create `GCRelayServer/Routing/RelayMember.cs`: bound member state.
- Create `GCRelayServer/Routing/RelayRoom.cs`: locked room membership.
- Create `GCRelayServer/Routing/RelayRoomRegistry.cs`: room lookup and expiry.
- Create `GCRelayServer/Routing/RelayTransmission.cs`: pure router output.
- Create `GCRelayServer/Routing/RelayRouter.cs`: membership and gameplay routing.
- Rewrite `GCRelayServer/RelayServer.cs`: thin receive/decode/route/encode adapter.
- Create `GCRelayServer/RelayServerOptions.cs`: validated standalone options.
- Create `GCRelayServer/RelayCleanupWorker.cs`: periodic expiry loop.
- Rewrite `GCRelayServer/Program.cs`: cancellation-based process lifecycle.
- Modify `GCRelayServer/GCRelayServer.csproj`: remove BinarySerializer.
- Modify `Directory.Packages.props`: remove the unused central BinarySerializer version.

---

### Task 1: Add Independent Codec Fixtures

**Files:**

- Modify: `Tests/MultiplayerTests/MultiplayerTests.csproj`
- Create: `Tests/MultiplayerTests/Relay/RelayDatagramCodecTests.cs`
- Create: `GCRelayServer/Protocol/RelayMessageType.cs`
- Create: `GCRelayServer/Protocol/RelayDatagram.cs`
- Create: `GCRelayServer/Protocol/RelayRejection.cs`
- Create: `GCRelayServer/Protocol/RelayProtocol.cs`
- Create: `GCRelayServer/Protocol/RelayDatagramCodec.cs`

- [ ] **Step 1: Reference the relay executable from the focused tests**

Add:

```xml
<ProjectReference Include="..\..\GCRelayServer\GCRelayServer.csproj" />
```

- [ ] **Step 2: Write failing byte-fixture tests**

Construct fixtures manually in test code rather than calling the production
encoder. Cover a 44-byte `B0`, a 292-byte `A6`, and one gameplay datagram.
Assert these exact decoded fields:

```csharp
HeaderSize == 12
Channel == 6
Subtype == RelayMessageType.HeartbeatRequest
Mode == 0
PayloadSize == 8
MatchingId, EntryNumber, SenderMachineId, TargetMachineId
```

Add round-trip equality and table-driven rejection fixtures for:

- fewer than 36 bytes and more than 8,192 bytes;
- header size other than 12;
- wrong remaining size;
- channel other than 6;
- nonzero channel/reserved outer values;
- payload-size/total-size mismatch;
- unknown subtype;
- client-sent `A1`, `A2`, or `B1`;
- every invalid subtype mode/payload combination.

- [ ] **Step 3: Run tests to verify RED**

Run: `dotnet test Tests/MultiplayerTests/MultiplayerTests.csproj -c Release --filter FullyQualifiedName~RelayDatagramCodecTests`

Expected: compilation fails because protocol types do not exist.

- [ ] **Step 4: Add the complete subtype enum and immutable datagram**

The enum includes:

```csharp
PhaseBarrier = 0x0010,
MusicSelection = 0x0030,
MusicSelectionCommit = 0x0031,
DifficultyAndItem = 0x0032,
LiveTuneSnapshot = 0x0040,
ResultRecord = 0x0050,
PresetMessage = 0x0060,
GuestLiveness = 0x0070,
HostStatus = 0x0071,
Registration = 0x00A0,
RegistrationAck = 0x00A1,
NewMember = 0x00A2,
Leave = 0x00A3,
Announcement = 0x00A6,
Start = 0x00A7,
HeartbeatRequest = 0x00B0,
HeartbeatResponse = 0x00B1
```

`RelayDatagram` stores private copied payload bytes, exposes
`ReadOnlyMemory<byte> Payload`, and supplies factory methods for changed
subtype/mode without mutable public fields.

- [ ] **Step 5: Implement descriptor-driven validation**

Encode the exact rules once:

| Type | Mode | Payload | Direction/role |
| --- | --- | ---: | --- |
| `0010` | 4 | 4 | client, any member |
| `0030` | 1 / 2 | 12 | host / guest |
| `0031` | 4 | 12 | client, any member |
| `0032` | 4 | 8 | client, any member |
| `0040` | 1 / 2 | 84 / 24 | host / guest |
| `0050` | 4 | 88 | client, any member |
| `0060` | 4 | 4 | client, any member |
| `0070` | 2 | 8 zero | guest |
| `0071` | 1 | 16 | host |
| `A0` | 0 | 256 | client membership |
| `A1` | 0 | 256 | relay output only |
| `A2` | 4 | 256 | relay output only |
| `A3` | 0 | 8 zero | client membership |
| `A6` | 4 | 256 | client membership |
| `A7` | 1 | 8 zero | host |
| `B0` | 0 | 8 zero | client heartbeat |
| `B1` | 0 | 8 zero | relay output only |

- [ ] **Step 6: Implement explicit big-endian decode/encode**

Use `BinaryPrimitives.ReadUInt16BigEndian`/`ReadUInt32BigEndian` at offsets
`0x00..0x20`. Decode requires exact total length `36 + payload_size`; encode
allocates that exact length and writes zero outer channel/reserved fields.
Return a bounded `RelayRejection` instead of throwing for remote input.

- [ ] **Step 7: Run tests to verify GREEN**

Run: `dotnet test Tests/MultiplayerTests/MultiplayerTests.csproj -c Release --filter FullyQualifiedName~RelayDatagramCodecTests`

Expected: all fixture and rejection tests pass.

- [ ] **Step 8: Commit**

```powershell
git add -- Tests/MultiplayerTests/MultiplayerTests.csproj Tests/MultiplayerTests/Relay/RelayDatagramCodecTests.cs GCRelayServer/Protocol
git commit -m "Add explicit relay datagram codec"
```

---

### Task 2: Replace Endpoint-Only Rooms with Bound Membership

**Files:**

- Create: `Tests/MultiplayerTests/Relay/RelayRoomRegistryTests.cs`
- Create: `GCRelayServer/Routing/RelayProfileIdentity.cs`
- Create: `GCRelayServer/Routing/RelayMember.cs`
- Create: `GCRelayServer/Routing/RelayRoom.cs`
- Create: `GCRelayServer/Routing/RelayRoomRegistry.cs`
- Delete: `GCRelayServer/Room.cs`
- Delete: `GCRelayServer/RoomManager.cs`

- [ ] **Step 1: Write failing membership tests**

Cover:

```csharp
[Fact]
public void FirstMember_BindsMachineEntryEndpointAndProfileIdentity();

[Fact]
public void ExactReannouncement_RefreshesDynamicProfileAndActivity();

[Theory]
public void ConflictingMachineEntryEndpointOrProfile_IsRejected(...);

[Fact]
public void FifthMember_IsRejectedWithoutClearingExistingMembers();

[Fact]
public void RemoveLastMember_RemovesRoom();

[Fact]
public void EvictExpiredRooms_UsesInjectedTimeProvider();
```

Build 256-byte profiles whose event ID at payload `0x0C` and raw 16-byte card
ID at `0x30` can be varied independently.

- [ ] **Step 2: Run tests to verify RED**

Run: `dotnet test Tests/MultiplayerTests/MultiplayerTests.csproj -c Release --filter FullyQualifiedName~RelayRoomRegistryTests`

Expected: compilation fails because the routing membership types do not exist.

- [ ] **Step 3: Parse the stable profile fingerprint**

`RelayProfileIdentity` reads the big-endian event ID and retains the exact raw
16 card-ID bytes for equality. It never logs or converts the card bytes to
unbounded user-facing text.

- [ ] **Step 4: Implement locked room membership**

`RelayRoom` owns one `Lock`, dictionaries by machine and entry, and a maximum of
four. Registration requires nonzero machine ID, entry `0..3`, a unique
machine/entry pair, endpoint consistency, and stable profile identity. Known
members may refresh dynamic profile bytes and last-seen time.

- [ ] **Step 5: Implement registry and expiry**

`RelayRoomRegistry` indexes by nonzero matching ID, accepts an injected
`TimeProvider` and idle duration, removes empty rooms, and returns an eviction
count. It does not create rooms for heartbeat traffic.

- [ ] **Step 6: Remove old endpoint-only types and verify GREEN**

Run: `dotnet test Tests/MultiplayerTests/MultiplayerTests.csproj -c Release --filter FullyQualifiedName~RelayRoomRegistryTests`

Expected: all identity, capacity, and manual-time expiry tests pass.

- [ ] **Step 7: Commit**

```powershell
git add -- GCRelayServer/Room.cs GCRelayServer/RoomManager.cs GCRelayServer/Routing Tests/MultiplayerTests/Relay/RelayRoomRegistryTests.cs
git commit -m "Bind relay room membership identities"
```

---

### Task 3: Implement Membership and Heartbeat Routing

**Files:**

- Create: `Tests/MultiplayerTests/Relay/RelayMembershipRouterTests.cs`
- Create: `GCRelayServer/Routing/RelayTransmission.cs`
- Create: `GCRelayServer/Routing/RelayRouter.cs`

- [ ] **Step 1: Write failing public-lifecycle tests**

Use three fixed endpoints/machines and assert exact transmission target,
subtype, mode, sender, entry, matching ID, target field, and payload:

```csharp
[Fact]
public void Heartbeat_EmitsOneB1WithoutCreatingRoom();

[Fact]
public void FirstA6_RegistersWithoutOutboundTransmission();

[Fact]
public void NewA6_EmitsA2OnlyToExistingMembers();

[Fact]
public void KnownA6_RemainsA6AndReachesEveryOtherMember();

[Fact]
public void LostA6_A0FallbackRegistersSendsA2ThenOneA1();

[Fact]
public void KnownA0_EmitsOneA1ToSender();

[Fact]
public void A3_RemovesAndIsNotForwarded();

[Fact]
public void A7_RequiresEntryZeroAndBroadcastsToOthers();
```

- [ ] **Step 2: Run tests to verify RED**

Run: `dotnet test Tests/MultiplayerTests/MultiplayerTests.csproj -c Release --filter FullyQualifiedName~RelayMembershipRouterTests`

Expected: compilation fails because the router/output types do not exist.

- [ ] **Step 3: Add pure routing outputs**

```csharp
public sealed record RelayTransmission(IPEndPoint Target, RelayDatagram Datagram);

public sealed record RelayRouteResult(
    RelayRouteDisposition Disposition,
    RelayRejection? Rejection,
    IReadOnlyList<RelayTransmission> Transmissions);
```

The router never sends directly and never mutates the input datagram.

- [ ] **Step 4: Implement `B0`, `A6`, and `A0`**

- `B0`: validate statelessly and return one same-source `B1` without a room.
- new `A6`: register, then create mode-4 `A2` copies for existing endpoints.
- known `A6`: refresh and forward unchanged to other members.
- new `A0`: register loss-recovery membership, send mode-4 `A2` to existing
  members, then one `A1` to the source.
- known `A0`: refresh and send one `A1`.

Preserve matching, entry, sender, route, target, and payload values unless the
documented response subtype/mode must change.

- [ ] **Step 5: Implement `A3` and `A7`**

`A3` requires exact membership, removes the member, and returns no
transmission. `A7` requires registered entry zero and broadcasts unchanged to
all other members.

- [ ] **Step 6: Run tests to verify GREEN**

Run: `dotnet test Tests/MultiplayerTests/MultiplayerTests.csproj -c Release --filter FullyQualifiedName~RelayMembershipRouterTests`

Expected: all lifecycle tests pass with exactly one acknowledgement where specified.

- [ ] **Step 7: Commit**

```powershell
git add -- GCRelayServer/Routing/RelayTransmission.cs GCRelayServer/Routing/RelayRouter.cs Tests/MultiplayerTests/Relay/RelayMembershipRouterTests.cs
git commit -m "Implement relay membership lifecycle"
```

---

### Task 4: Route Every Gameplay and Control Message

**Files:**

- Create: `Tests/MultiplayerTests/Relay/RelayGameplayRouterTests.cs`
- Modify: `GCRelayServer/Protocol/RelayProtocol.cs`
- Modify: `GCRelayServer/Routing/RelayRouter.cs`

- [ ] **Step 1: Write failing table-driven routing tests**

For every client-built gameplay subtype (`0010`, `0030`, `0031`, `0032`,
`0040`, `0050`, `0060`, `0070`, `0071`), construct the exact valid mode and
payload and assert broadcast routing excludes the sender.

Add cases proving:

- target `0` and `0xFFFFFFFF` broadcast;
- a concrete target reaches only that machine;
- an unknown concrete target produces no transmission and a rejection;
- nonmembers, wrong endpoints, wrong entries, and mismatched machine IDs are
  rejected;
- entry zero alone may send host modes `0030/1`, `0040/1`, and `0071`;
- nonzero entries alone may send guest modes `0030/2`, `0040/2`, and `0070`;
- a rejected packet does not refresh room activity or change membership.

- [ ] **Step 2: Run tests to verify RED**

Run: `dotnet test Tests/MultiplayerTests/MultiplayerTests.csproj -c Release --filter FullyQualifiedName~RelayGameplayRouterTests`

Expected: unimplemented default routing fails.

- [ ] **Step 3: Implement registered-sender routing**

Resolve and validate the sender before choosing targets. Use the descriptor
role rule against stable entry zero/nonzero. Route an unchanged immutable
datagram to the selected member endpoints and touch activity only after all
validation succeeds.

- [ ] **Step 4: Reject unsupported/default behavior**

There is no opaque default broadcast. Unknown, output-only, invalid-role, and
unregistered messages return explicit bounded rejection dispositions.

- [ ] **Step 5: Run tests to verify GREEN**

Run: `dotnet test Tests/MultiplayerTests/MultiplayerTests.csproj -c Release --filter FullyQualifiedName~RelayGameplayRouterTests`

Expected: all subtype, target, role, and spoof tests pass.

- [ ] **Step 6: Commit**

```powershell
git add -- GCRelayServer/Protocol/RelayProtocol.cs GCRelayServer/Routing/RelayRouter.cs Tests/MultiplayerTests/Relay/RelayGameplayRouterTests.cs
git commit -m "Route validated relay gameplay messages"
```

---

### Task 5: Make the UDP Server a Thin Adapter

**Files:**

- Rewrite: `GCRelayServer/RelayServer.cs`
- Delete: `GCRelayServer/RelayPacket.cs`
- Delete: `GCRelayServer/RelayPacketTypes.cs`
- Modify: `GCRelayServer/GCRelayServer.csproj`
- Modify: `Directory.Packages.props`

- [ ] **Step 1: Replace receive handling**

In `OnReceived`:

1. Require `IPEndPoint` and checked `int` offset/size.
2. Decode `buffer.AsSpan(offset, size)` with `RelayDatagramCodec`.
3. Log and return on a bounded rejection.
4. Pass the decoded datagram to `RelayRouter`.
5. Encode each returned transmission into a fresh exact-length byte array.
6. Call `SendAsync` once per transmission.
7. Always rearm `ReceiveAsync()` in `finally`.

No network callback may mutate a shared packet object.

- [ ] **Step 2: Preserve structured socket lifecycle logging**

Log subtype, matching ID, sender machine/entry, source, disposition, and target
count at debug level. Do not dump profile payloads or card IDs. Retain error
logging and receive rearming.

- [ ] **Step 3: Remove the reflection serializer**

Delete `RelayPacket.cs` and `RelayPacketTypes.cs`, remove the
`BinarySerializer` package reference from `GCRelayServer.csproj`, and remove
its unused central version from `Directory.Packages.props`.

- [ ] **Step 4: Run relay and codec tests**

Run: `dotnet test Tests/MultiplayerTests/MultiplayerTests.csproj -c Release --filter FullyQualifiedName~Relay`

Expected: all pure protocol/routing tests pass and GCRelayServer builds without BinarySerializer.

- [ ] **Step 5: Commit**

```powershell
git add -- GCRelayServer/RelayServer.cs GCRelayServer/RelayPacket.cs GCRelayServer/RelayPacketTypes.cs GCRelayServer/GCRelayServer.csproj Directory.Packages.props
git commit -m "Use validated relay routing adapter"
```

---

### Task 6: Add Configurable Scheduled Expiry and Graceful Shutdown

**Files:**

- Create: `Tests/MultiplayerTests/Relay/RelayServerOptionsTests.cs`
- Create: `GCRelayServer/RelayServerOptions.cs`
- Create: `GCRelayServer/RelayCleanupWorker.cs`
- Rewrite: `GCRelayServer/Program.cs`

- [ ] **Step 1: Write failing option tests**

Cover positional compatibility and named validation:

```csharp
[Fact]
public void NoArguments_UsesPort3333TenMinuteExpiryAndFiveSecondCleanup();

[Fact]
public void PositionalPort_RemainsSupported();

[Fact]
public void NamedOptions_OverridePortExpiryAndCleanup();

[Theory]
public void InvalidPortOrNonPositiveDuration_IsRejected(string[] args);
```

- [ ] **Step 2: Run tests to verify RED**

Run: `dotnet test Tests/MultiplayerTests/MultiplayerTests.csproj -c Release --filter FullyQualifiedName~RelayServerOptionsTests`

Expected: compilation fails because options do not exist.

- [ ] **Step 3: Implement standalone options**

Support the legacy first positional port plus:

```text
--port <1..65535>
--room-expiry-seconds <positive integer>
--cleanup-interval-seconds <positive integer>
```

Return a parse error rather than silently replacing an explicit invalid value.

- [ ] **Step 4: Implement periodic cleanup**

`RelayCleanupWorker.RunAsync(CancellationToken)` owns a
`PeriodicTimer(cleanupInterval, timeProvider)` and invokes
`RelayRoomRegistry.EvictExpiredRooms()` on each tick. Cancellation is normal
shutdown, not an error.

- [ ] **Step 5: Rewrite process lifecycle**

Use `static async Task Main`. Parse options, create `TimeProvider.System`,
registry, router, and UDP server, then start the cleanup worker. Ctrl+C cancels
one `CancellationTokenSource`; shutdown stops the UDP server and awaits the
worker. Do not rely on `Console.ReadLine()` so redirected/noninteractive
deployments remain alive and cleanly cancellable.

- [ ] **Step 6: Run tests to verify GREEN**

Run: `dotnet test Tests/MultiplayerTests/MultiplayerTests.csproj -c Release --filter FullyQualifiedName~Relay`

Expected: all relay tests pass.

- [ ] **Step 7: Commit**

```powershell
git add -- GCRelayServer/RelayServerOptions.cs GCRelayServer/RelayCleanupWorker.cs GCRelayServer/Program.cs Tests/MultiplayerTests/Relay/RelayServerOptionsTests.cs
git commit -m "Schedule relay room expiry"
```

---

### Task 7: Verify the Full Multiplayer Pipeline

**Files:**

- Modify only if verification exposes a defect in files owned by the two multiplayer plans.

- [ ] **Step 1: Run focused multiplayer tests repeatedly**

Run: `dotnet test Tests/MultiplayerTests/MultiplayerTests.csproj -c Release`

Run the same command a second time.

Expected: every test passes twice with no wall-clock or concurrency flake.

- [ ] **Step 2: Run formatting verification**

Run: `dotnet format GC-local-server-rewrite.slnx --verify-no-changes`

Expected: no formatting changes required.

- [ ] **Step 3: Run the full solution**

Run: `dotnet restore GC-local-server-rewrite.slnx`

Run: `dotnet build GC-local-server-rewrite.slnx -c Release --no-restore`

Run: `dotnet test GC-local-server-rewrite.slnx -c Release --no-build`

Expected: restore succeeds; build has no new warnings/errors; all tests pass.

- [ ] **Step 4: Audit obsolete patterns and scope**

Run: `rg -n "BinarySerialization|BinarySerializer|endpoints.Clear|SendPacket\(sender.*SendPacket\(sender|default:\s*HandleRelay" GCRelayServer Directory.Packages.props`

Expected: no obsolete serializer, room-clearing, duplicate-ack, or opaque-default-broadcast path remains.

Run: `git status --short`

Expected: only intentional multiplayer-plan changes, if any, remain uncommitted.

- [ ] **Step 5: Preserve the runtime acceptance boundary**

Static tests prove codec bytes, state transitions, validation, and routing
decisions. Do not claim real-cabinet or official-service parity until two-,
three-, and four-cabinet sessions and timestamped official HTTP/relay captures
have exercised the documented acceptance matrix.
