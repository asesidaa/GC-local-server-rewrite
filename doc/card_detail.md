# Card detail table

card_id, pcol1, pcol2, pcol3 are primary keys

| pcol1 |  pco2   |   pcol3    |                           Function                           |
| :---: | :-----: | :--------: | :----------------------------------------------------------: |
|   0   |    0    |     0      | score_i1:**avatar** score_ui1:**fast/slow** score_ui2:**fever/trance** fcol1: fcol2:**title** fcol3:**sound** |
|   1   |    0    |     0      |           score_i1:**navigator** fcol3:**unknown**           |
|  10   | song id |     0      | score_i1:**skin**, score_ui2:**Whether the song is unlocked**, score_ui6: **The song need to be unlocked**, fcol1:**Favorite song** |
|  20   | song id | difficulty | score_i1: **rank(S and above=0,A=1,etc)**,**score_ui1:**play count** score_ui2:**clear count** score_ui3:**no miss or higher count** score_ui4: **full_chain or higher count** score_ui5: **1 for S+,2 for S++** score_ui6: **perfect count** |
|  21   | song id | difficulty | score_ui1/score_ui5: **highest score**, score_ui2/score_ui6:**highest_score time** score_ui3:**max_chain** score_ui4: **max chain time** fcol1:**max hit adlib count** fcol2:**time for fcol1** |
|  30   |    0    |     0      |     score_ui2:**Unknown, looks like some sort of count**     |
|  31   |    0    |     0      |    score_bi1: **Unknown, looks like some sort of count**     |

