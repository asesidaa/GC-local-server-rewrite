# Card Detail Music Flag Correction Design

## Context

The updated `doc/card_detail.md` establishes the `pcol1 = 10` music-detail contract:

- `score_ui2` is the per-card song-unlocked flag.
- `score_ui6` is set only while the song is available or unlocked and has no `pcol1 = 20` play record.

Two application paths still encode the previous interpretation of `score_ui6`. `MaintainNullValuesJob` rewrites music flags every 24 hours, and `UnlockAllMusicCommand` bulk-upserts replacement rows with both flags forced to `1`. The latter can mark played songs as unplayed and overwrite existing per-song skin, favorite, and metadata fields.

The remaining null-repair responsibility of `MaintainNullValuesJob` is obsolete. Current card-detail writes always populate store and time metadata, nullable legacy values are supported by the schema, Mapperly retains the DTO defaults when those values are null, and ranking queries already tolerate missing values.

## Goals

- Remove the obsolete scheduled maintenance job and its Quartz registration.
- Keep the existing Unlock All API and lock-system bitset behavior.
- Set `score_ui2 = 1` for every configured music reward targeted by Unlock All.
- Set `score_ui6 = 0` when any `pcol1 = 20` record exists for that card and music ID; otherwise set it to `1`.
- Preserve every unrelated field on existing `pcol1 = 10` rows.
- Create compatibility rows only when a configured reward song has no existing `pcol1 = 10` row.

## Non-Goals

- Changing the game-protocol DTO or database schema.
- Reinterpreting or normalizing client-authored music-detail rows during ordinary card reads or writes.
- Changing reward configuration, the music catalog, global `use_flag`/`new_flag`, authorization, or card-existence behavior.
- Expanding Unlock All compatibility rows beyond configured `RewardType.Music` rewards.

## Application Changes

`Application/Jobs/MaintainNullValuesJob.cs` will be deleted. Its `AddJob` and 24-hour trigger block will be removed from `Application/DependencyInjection.cs`. Other ranking and cleanup jobs remain unchanged.

`UnlockAllMusicCommandHandler` will continue to set every bit in the per-player music bitset when the lock system is enabled. For compatibility card details, it will:

1. Read the distinct configured music-reward IDs.
2. Read existing `pcol1 = 10`, `pcol3 = 0` rows for the requested card and those IDs.
3. Read the distinct music IDs having any `pcol1 = 20` play record for the requested card.
4. Update existing music-detail rows in place, setting `score_ui2 = 1` and deriving `score_ui6` from play-record absence.
5. Add a new row only for a configured ID that has no existing music-detail row, using the same flag rule and the current compatibility metadata defaults.
6. Save the bitset and card-detail changes together through the existing context.

Updating tracked entities in place avoids the current full-row upsert, so selected skin, favorite state, timestamps, and other protocol fields survive Unlock All.

## Testing

Focused xUnit integration tests will use temporary SQLite contexts and the real handler:

- Verify an existing played reward song becomes `(score_ui2, score_ui6) = (1, 0)` while its skin, favorite, and metadata fields remain unchanged.
- Verify an existing unplayed reward song becomes `(1, 1)` without losing unrelated fields.
- Verify a missing compatibility row is created with `score_ui6` derived from whether a play record already exists.
- Verify the Application Quartz schedule no longer contains `MaintainNullValuesJob`.

The focused tests will be run red before production edits, then green after the implementation. Final verification will run the complete Release test suite and Release solution build.

## Risks and Controls

- **Duplicate reward configuration:** de-duplicate music IDs before querying or creating rows.
- **Multiple difficulty records:** treat any `pcol1 = 20` row for the music ID as played, independent of `pcol3` difficulty.
- **Existing customization loss:** mutate tracked rows rather than replacing them with DTO defaults.
- **Unintended scheduled-job removal:** remove only `MaintainNullValuesJob`; keep all other Quartz registrations intact and cover the schedule through service registration.
- **Working-tree overlap:** stage and commit only workflow-owned files; leave the user's existing documentation and launch-settings changes untouched.

## Success Criteria

The change is complete when Unlock All preserves existing music-detail data, produces `(1, 0)` for played reward songs and `(1, 1)` for unplayed reward songs, the obsolete maintenance job no longer runs, and all focused tests, solution tests, and the Release build pass.
