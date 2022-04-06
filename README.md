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

The config file is GC-local-server-rewrite.exe.config.

If you are using 4EX data, it should work out of box.

If you are using 4MAX data, first change MusicDBName to bundled music4MAX.db3

To unlock new avatars/navgators/skins/sound effects/titles from 4MAX, change the corresponding count in config.

You can get the corresponding count in data/boot/*.dat file.

# Missing functions

- [ ] Item/coin comsuming 
- [ ] Unlocking system (I don't like these two so I just hardcode them, if you are interested you can implement that and add a switch to enable, PRs are welcome)
- [ ] Ranking system (Is this really needed for a local server?) 
- [ ] Proper update check response (Now it will just throw 404)
- [ ] PlayInfo.php (Now it will just throw 404, I don't know what data it expext)

# Difficulty unlocking

This is processed on client side, so if you like to unlock all difficuties, just use bemani patcher.

# Local network

If your game and server is not on the same computer, try modify in config, change ServerIp to your server IP.

From server, open mmc.exe, export certificate named "Taito Arcade Machine CA" and "GC local server" with private key and import to game computer.

The cert "Taito Arcade Machine CA" goes into LocalMachine/My and Trusted root, the other only LocalMachine/My.

Note: this is currently not tested.
