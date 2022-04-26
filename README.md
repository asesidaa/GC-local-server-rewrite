# GC local server

This is a rewritten version of GC local  server, original version is [here](https://github.com/asesidaa/gc-local-server).

Server functionality is basically the same, but the code should be now easier to work with, and can be extended esaily.

Now it supports card registration.

# Usage

Download latest release from release page.

Extract anywhere.

If you don't have the modified NesysService.exe, modify the host file, add the following lines

```
127.0.0.1 cert.nesys.jp
127.0.0.1 data.nesys.jp
127.0.0.1 nesys.taito.co.jp
127.0.0.1 fjm170920zero.nesica.net
```

Open mmc.exe, delete any certificate in LocalMachine/My or LocalMachine/Trusted root named Taito Arcade Machine CA (if present, most likely not)

Open exe with **admin privileges** for certificate generating functionalities to work.

Open game using teknoparrot loader (you can find that in discord). The loader should be also opened with **admin privileges**, otherwise it will not work.

# Config

The config file is ~~GC-local-server-rewrite.exe.config~~ config.json

If you are using 4MAX 4.65 data, it should work out of box.

If you are using 4MAX 4.61 data, first change MusicDbName to bundled music4MAX.db3

If you are using 4EX 4.52 data, change MusicDbName to bundled music.db3

To unlock new avatars/navgators/skins/sound effects/titles from other version, change the corresponding count in config.

You can get the corresponding count in data/boot/*.dat file.

# Event files

You can now add solo events. In the config file, change the following section

```
  "ResponseData": [
    {
      "FileName": "/event_031_20160112.evt", // This is the file name of the file to be read
      "NotBeforeUnixTime": 1272260187, 
      "NotAfterUnixTime": 1903412187,// Be sure that NotBeforeUnixTime<= current time <= NotAfterUnixTime
      "Md5": "28a12ed884747db261b188bc2c97c555" // File MD5, must match
    },
    {
      "FileName": "/telop_20201125.txt",
      "NotBeforeUnixTime": 1272260187,
      "NotAfterUnixTime": 1903412187,
      "Md5": "86fb269d190d2c85f6e0468ceca42a20"
    }
  ]
```

There are several types of data. File name starts with `event` is event config file. Sample 

```
event_id = 1
title = "Test event"
comment = ""
open_date = (2021, 10, 12)
close_date = (2022, 12, 24)
open_time = (8, 0, 0)
close_time = (23, 59, 0)
ver = "4.65.00"
play_open_time = (0, 0, 0)
play_close_time = (0, 0, 0)
type = 0
challenge_num = 0
play_mode = 1 // 1 is solo, 0 is online so currently not availabe

select_music_data = (185, 191, 257, 261, 263, 267, 357, 358, 363, 364, 412, 416, 419, 494, 517, 518, 558, 572, 578, 579, 580, 699, 712, 766, 775, 789, 811, 812, 813, 814) // Enabled music

// Reward is not implemented, also everything has already been unlocked
reward0.type = 2
reward0.start = 1
reward0.end = 50
reward0.title = 5159
reward0.avatar = 0
reward0.item = 21
reward0.item_num = 3
reward0.trophy_num = 2
reward0.music = 0
reward0.navigator = 0 
```

File name starts with `telop` is the scrolling banner, it's just a plain text file with banner content.

Other files like `news`, `option` and `cap` should also be possible, but I have not tested these.

The configured file should be put into the `event` folder, which you can change the position by using jconfig (regedit->event path)

# Missing functions

- [ ] Item/coin consuming 
- [ ] Unlocking system (I don't like these two so I just hardcode them, if you are interested you can implement that and add a switch to enable, PRs are welcome)
- [ ] Ranking system (Is this really needed for a local server?) 
- [ ] Proper update check response (Now it will just throw 404)
- [ ] PlayInfo.php (Now it will just throw 404, I don't know what data it expext)
- [ ] Online matching (and online matching events)

# Difficulty unlocking

This is processed on client side, so if you like to unlock all difficuties, just use bemani patcher.

# Deteled songs

If you see a lot of duplicate "Play Merrily" in the original tab, this is because in the songs db deleted songs are added back.

To enable these, try use the omnimixed version of stage_param.dat. That can fix this issue

# Local network

If your game and server is not on the same computer, try modify in config, change ServerIp to your server IP.

From server, open mmc.exe, export certificate named "Taito Arcade Machine CA" and "GC local server" with private key and import to game computer.

The cert "Taito Arcade Machine CA" goes into LocalMachine/My and Trusted root, the other only LocalMachine/My.

# Windows XP

If you are using Windows XP (e.g. using real machine), it will not recognize the generated certificate since it uses SHA256.

You will have to generate the certificates yourself. 

The root certificate should have CN=Taito Arcade Machine CA, while the server certificate should have DNS entries for the domains in the host file.

The most important bit, **choose MD5 or SHA1 as signature algorithm**.
