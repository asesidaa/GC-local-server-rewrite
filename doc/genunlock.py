import csv
import sqlite3
import json


con = sqlite3.connect('music471omni.db3')
cur = con.cursor()
id = 6
with open('unlockables.csv', encoding='utf8') as csvfile:
  reader = csv.reader(csvfile, delimiter=',')
  for row in reader:
    name = row[0]
    res = cur.execute("SELECT music_id FROM music_unlock where title=:t", {"t":name})
    for music_id in res:
      config = {
        "RewardId": id,
        "RewardType": "Music",
        "TargetId": music_id[0],
        "TargetNum": 1,
        "KeyNum": 1
    }
      id += 1
      print(json.dumps(config, indent=4) + ",")

