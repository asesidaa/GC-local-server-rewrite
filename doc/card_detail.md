# Card detail table

`card_id`, `pcol1`, `pcol2`, and `pcol3` are primary keys.

| `pcol1` | `pcol2` | `pcol3` | Used fields |
| :---: | :---: | :---: | --- |
| 0 | 0 | 0 | `score_i1`: **selected avatar ID**<br>`score_ui1`: **Fast/Slow display option**<br>`score_ui2`: **Fever/Trance display option**<br>`fcol2`: **selected title ID**<br>`fcol3`: **selected AD-LIB sound-effect ID** |
| 1 | 0 | 0 | `score_i1`: **selected navigator ID**<br>`score_ui3`: **opponents defeated in local play**<br>`score_ui4`: **duplicate opponents-defeated total in local play**<br>`score_ui5`: **opponents defeated in event play**<br>`score_ui6`: **opponents faced in event play**<br>`fcol3`: **previous total-trophy count used for reward-threshold checks** |
| 10 | Music ID | 0 | `score_i1`: **selected skin ID for this song**<br>`score_ui2`: **song unlocked flag**<br>`score_ui6`: **song is available/unlocked but has no play record yet**<br>`fcol1`: **favorite song flag** |
| 20 | Music ID | Difficulty | `score_i1`: **best base rank (`0=S`, `1=A`, `2=B`, `3=C`, `4=D`, `5=E`, `6=X/unranked`)**<br>`score_ui1`: **play count**<br>`score_ui2`: **clear count**<br>`score_ui3`: **no-miss-or-better count**<br>`score_ui4`: **full-chain-or-better count**<br>`score_ui5`: **highest S suffix (`0=none`, `1=S+`, `2=S++`)**<br>`score_ui6`: **perfect count** |
| 21 | Music ID | Difficulty | `score_ui1`: **highest total score**<br>`score_ui2`: **timestamp for `score_ui1`**<br>`score_ui3`: **maximum chain**<br>`score_ui4`: **timestamp for `score_ui3`**<br>`score_ui5`: **duplicate highest total score in v4.71**<br>`score_ui6`: **timestamp for `score_ui5`**<br>`fcol1`: **maximum hit-ADLIB count**<br>`fcol2`: **timestamp for `fcol1`** |
| 30 | 0 | 0 | `score_ui2`: **consecutive no-miss-stage streak**<br>`score_bi1`: **Japanese-prefecture play-history bitset** |
| 31 | 0 | 0 | `score_bi1`: **total item-assisted clears (low 32 bits)**<br>`score_i1`: **MIRROR clears (item 4)**<br>`score_ui1`: **NO WAY clears (item 5)**<br>`score_ui2`: **REVERSE clears (item 6)**<br>`score_ui3`: **HIDDEN clears (item 7)**<br>`score_ui4`: **SUDDEN clears (item 8)**<br>`score_ui5`: **JUST clears (item 9)**<br>`score_ui6`: **STEALTH clears (item 10)**<br>`fcol2`: **NOTHING clears (item 12)**<br>`fcol3`: **IMPOSSIBLE clears (item 13)** |
| 100 | 0 | 0 | `score_i1`: **event attempts already used**<br>`score_ui6`: **cumulative event points (EP)** |
