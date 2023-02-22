# GC local server

This is a rewritten version of GC local server, original version is [here](https://github.com/asesidaa/gc-local-server).

**Now migrated to asp .net core**

Server functionality is basically the same, but the code should be now easier to work with, and can be extended easily.

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

Open exe with **admin privileges** for certificate generating functionalities to work.

Open game using openparrot loader (you can find that in guide). The loader should be also opened with **admin privileges**, otherwise it will not work.

## Config

New config is distributed in files located in the `Configurations` folder.

- `database.json`: This file controls which db file to use. `CardDbName` for main card database, `MusicDbName` for music database. Remember to switch music databases for different versions.

- `events.json`: This file controls which event files to use. All the possible event file types are listed. 

  `.evt` file is the main event file, `play_mode` controls solo or multiplayer

  `_reg.jpg` is the big event news picture

  `_sgreg.jpg` is the small event news picture shown in mode selection

  `news_big_*.jpg` is the big news picture shown before demo. The first one must have index 0, other ones can have index 2~9

  `news_small_*.jpg` is the small news picture shown after game over

  `telop_*.txt` is the text for the scrolling banner

  `*.cmp` is the file controlling additional configs, refer to comments

  When you want to use event files, put them in `wwwroot/events` folder, then change `UseEvents` to true

- `game.json`: This files controls settings for game. You can get the corresponding count for a specified version in `data/boot/*.dat` file. It is the first 2 bytes (uint16) encoded in big endian. Since everything is unlock by default, the `UnlockRewards` is just a demo for future unlock functionality.

- Other config files do not need to be modified currently

## Missing functions

- [ ] Item/coin consuming 
- [ ] Unlocking system
- [x] ~~Ranking system~~ Initial support for actual ranking
- [ ] Proper update check response (Now it will just throw 404)
- [ ] Online matching (and online matching events), would be added in next version

## Difficulty unlocking

This is processed on client side, so if you like to unlock all difficulties, just use Bemani patcher with 4.52 exe (they are compatible).

## Deleted songs

If you see a lot of duplicate "Play Merrily" in the original tab, this is because in the songs db deleted songs are added back.

To enable these, try use the omnimixed version of stage_param.dat. That can fix this issue

## Local network

If your game and server is not on the same computer, import the certificates in `Certificates`  folder. `root.pfx` goes into LocalMachine/My and Trusted root, the other only LocalMachine/My.

## Windows XP

If you are using Windows XP (e.g. using real machine), it will not recognize the generated certificate since it uses SHA256.

You will have to generate the certificates yourself. 

The root certificate should have CN=Taito Arcade Machine CA, while the server certificate should have DNS entries for the domains in the host file.

The most important bit, **choose MD5 or SHA1 as signature algorithm**.

# Web UI

There's a basic web interface for check scores and set options.

## Song unlock

To unlock all songs, first play for one time and save, then in web UI, go to `Edit Options` to unlock all songs.
