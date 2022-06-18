# Card Bdata table

Stores some binary card data, encoded in base64

| index | size | data                                      |
| ----- | ---- | ----------------------------------------- |
| 0     | 6    | fixed value "1.51"                        |
| 1     | 6    | Last played song and difficulty?          |
| 2     | 68   | Something consist of 17 ints              |
| 3     | 4    | Start related, unknown                    |
| 4     | 4    | Something whose max value is 100, unknown |
| 5     | 4    | Unknown                                   |
| 6     | 4    | Unknown                                   |
| 7     | 4    | Unknown                                   |
| 8     | 4    | Always 0                                  |
| 9     | 160  | 40 previously newly played song id        |
| 10    | 16   | Filetime + stamp count                    |

