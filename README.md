# GC local server

This is a rewritten version of GC local  server, original version is [here](https://github.com/asesidaa/gc-local-server).

Server functionality is basically the same, but the code should be now easier to work with, and can be extended esaily.

Now it supports card registration.

## Usage

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

## Config

The config file is ~~GC-local-server-rewrite.exe.config~~ config.json

If you are using 4MAX 4.71 data, it should work out of box.

If you are using 4MAX 4.65 data, MusicDbName=music4MAX465.db3

If you are using 4MAX 4.61 data, first change MusicDbName to bundled music4MAX.db3

If you are using 4EX 4.52 data, change MusicDbName to bundled music.db3

To unlock new avatars/navgators/skins/sound effects/titles from other version, change the corresponding count in config.

You can get the corresponding count in data/boot/*.dat file.

## Event files

You can now add solo events. In the config file, change the following section

```
  "ResponseData": [
    {
      "FileName": "/event_103_20201125.evt", // This is the file name of the file to be read
      "NotBeforeUnixTime": 1335677127,
      "NotAfterUnixTime": 1966397127, // Be sure that NotBeforeUnixTime<= current time <= NotAfterUnixTime
      "Md5": "9ef6c4d5ca381583a2d99b626ce06b5e", // File MD5, must match
      "Index": 0 // Special value, must be 0 for .evt file, this is the file controlling event
    },
    {
      "FileName": "/event_20201125_reg.jpg",
      "NotBeforeUnixTime": 1335677127,
      "NotAfterUnixTime": 1966397127,
      "Md5": "8e3fe25bf50dcbed13dbb54cc18b1efa",
      "Index": 1 // Special value, must be 1 for event_reg.jpg
    },
    {
      "FileName": "/event_20201125_sgreg.png",
      "NotBeforeUnixTime": 1335677127,
      "NotAfterUnixTime": 1966397127,
      "Md5": "e0abb0503fe0c530d8a68e36994264c6",
      "Index": 2 // Special value, must be 2 for event_sreg.jpg, this is the event picture in mode selection
    },
    {
      "FileName": "/news_big_20201125_0.jpg",
      "NotBeforeUnixTime": 1335677127,
      "NotAfterUnixTime": 1966397127,
      "Md5": "4a0f66431f6449279dc046149d1dd882",
      "Index": 0 // Special value, must be 0 for first news_big.jpg, this is the picture shown before demo
    },
    {
      "FileName": "/news_big_20201125_2.jpg",
      "NotBeforeUnixTime": 1335677127,
      "NotAfterUnixTime": 1966397127,
      "Md5": "8e3fe25bf50dcbed13dbb54cc18b1efa",
      "Index": 2 // Special value, can be 2~9 for 2nd to 9th news_big.jpg, so there can be up to 9 of these
    },
    {
      "FileName": "/news_small_20201125_1.jpg",
      "NotBeforeUnixTime": 1335677127,
      "NotAfterUnixTime": 1966397127,
      "Md5": "e20135bcd41c98875aec2b52eb9fcd06",
      "Index": 1 // Special value, must be 1 for news_small.jpg, this is shown after game over
    },
    {
      "FileName": "/telop_20201125.txt",
      "NotBeforeUnixTime": 1335677127,
      "NotAfterUnixTime": 1966397127,
      "Md5": "ee228de44d6656a9ec0bb7f1a0ca64e1",
      "Index": 0 // Special value, must be 1 for telop, this is the text in scrolling banner
    },
    {
      "FileName": "/event_unlock_20201125.cmp",
      "NotBeforeUnixTime": 1335677127,
      "NotAfterUnixTime": 1966397127,
      "Md5": "623b0f10125cbe19c5394d992930ae8c",
      "Index": 8 // Special value, must be 8 for .cmp file
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

Other files like `news`, `option` and `cap` should also be possible, but I have not tested these.

The configured file should be put into the `event` folder, which you can change the position by using jconfig (regedit->event path)

## Missing functions

- [ ] Item/coin consuming 
- [ ] Unlocking system (I don't like these two so I just hardcode them, if you are interested you can implement that and add a switch to enable, PRs are welcome)
- [ ] Ranking system (Is this really needed for a local server?) 
- [ ] Proper update check response (Now it will just throw 404)
- [ ] PlayInfo.php (Now it will just throw 404, I don't know what data it expext)
- [ ] Online matching (and online matching events)

## Difficulty unlocking

This is processed on client side, so if you like to unlock all difficulties, just use Bemani patcher.

## Deteled songs

If you see a lot of duplicate "Play Merrily" in the original tab, this is because in the songs db deleted songs are added back.

To enable these, try use the omnimixed version of stage_param.dat. That can fix this issue

## Local network

If your game and server is not on the same computer, try modify in config, change ServerIp to your server IP.

From server, open mmc.exe, export certificate named "Taito Arcade Machine CA" and "GC local server" with private key and import to game computer.

The cert "Taito Arcade Machine CA" goes into LocalMachine/My and Trusted root, the other only LocalMachine/My.

## Windows XP

If you are using Windows XP (e.g. using real machine), it will not recognize the generated certificate since it uses SHA256.

You will have to generate the certificates yourself. 

The root certificate should have CN=Taito Arcade Machine CA, while the server certificate should have DNS entries for the domains in the host file.

The most important bit, **choose MD5 or SHA1 as signature algorithm**.

# Web interface

A basic web interface for check scores and set options.

If you want to use the interface besides localhost(127.0.0.1), change in appsettings.json:

```json
"BaseUrl": "http://192.168.1.1" // Change to your server ip
```

Notice that due to certificate issues, unless you have set up the certificate yourself, you need to use http for `BaseUrl` and when access the web page, otherwise it will fail to send requests.
