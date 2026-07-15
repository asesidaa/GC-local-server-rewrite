# Card Bdata table

`bdata` is a base64-encoded little-endian TLV stream. Each record contains a `uint16` index, a `uint16` size, and `size` bytes of data. The standard payload is 324 decoded bytes and 432 base64 characters. `bdata_size` is 446 because it stores the base64 length before the encoder's CR/LF line breaks are removed; it is not the decoded byte count.

| index | size | data |
| :---: | :---: | --- |
| 0 | 6 | Fixed format version: `"1.51\0\0"`. |
| 1 | 6 | `uint16`: **last song-select sort/category ID (`0-7`; `7=Favorites`)**<br>`uint16`: **last selected music ID**<br>`uint16`: **last selected difficulty (`0-3`)** |
| 2 | 68 | Seventeen `uint32` **successful judged-target counters** for targets with `ArrangeType=NONE`; each counter saturates at 99,999,999.<br>`[0]`: **NONE**<br>`[1]`: **NORMAL**<br>`[2]`: **FLICK**<br>`[3]`: **HOLD**<br>`[4]`: **SCRATCH**<br>`[5]`: **BEAT**<br>`[6]`: **MERRY GO ROUND**<br>`[7]`: **HIDDEN**<br>`[8]`: **HIDDEN2**<br>`[9]`: **CRITICAL**<br>`[10]`: **SLIDE HOLD**<br>`[11]`: **SLIDE COUNTER**<br>`[12]`: **TURN**<br>`[13]`: **SPIN**<br>`[14]`: **FINISH**<br>`[15]`: **DUAL HOLD**<br>`[16]`: **special NORMAL subtype with its special flag set and a nonzero associated 3-vector; this is not a separate `MarkType` or AD-LIB counter** |
| 3 | 4 | `int32`: **completed game-entry/session count**. This is not the number of songs or stages played. `-1` is a non-incrementing sentinel. |
| 4 | 4 | `uint32`: **headphone volume percentage (`0-100`)**. |
| 5 | 4 | `int32`: **BOOSTER vibration mode**. `-1` means unset/default; effective modes are `0-3`, mapped to device parameters `3`, `4`, `1`, and `2`. |
| 6 | 4 | `uint32`: **number of committed BOOSTER vibration-setting changes**. |
| 7 | 4 | `uint32`: **cached `new_flag` for the last selected song when sort/category 0 is active**. This only restores the previous list selection; authoritative NEW status comes from the music-detail `new_flag`. |
| 8 | 4 | `uint32`: **reserved/unused**, normally `0`. |
| 9 | 160 | Forty `uint32` **recently played music IDs**, newest first and zero-padded. Adding an ID removes its previous occurrence, shifts the remaining IDs, and drops the oldest entry. This is not a list of newly released songs. |
| 10 | 16 | `uint64`: **Windows FILETIME used for entry-stamp game-day comparison**<br>`uint32`: **entry count for the current game day, reset at the 08:00 boundary**<br>`uint32`: **reserved/unused, normally `0`** |

