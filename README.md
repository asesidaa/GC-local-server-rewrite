# GC local server

This is a rewritten version of GC local  server, original version is [here](https://github.com/asesidaa/gc-local-server).

Server functionality is basically the same, but the code should be now easier to work with, and can be extended esaily.

Now it supports card registration.

# Usage

Download latest release from release page.

Extract anywhere.

Open exe with **admin privileges** for certificate generating functionalities to work.

Open game using teknoparrot loader (you can find that in discord). The loader should be also opened with **admin privileges**, otherwise it will not work.

# Config

The config file is GC-local-server-rewrite.exe.config.

If you are using 4EX data, it should work out of box.

If you are using 4MAX data, first change MusicDBName to bundled music4MAX.db3

To unlock new avatars/navgators/skins/sound effects/titles from 4MAX, change the corresponding count in config.

You can get the corresponding count in data/boot/*.dat file.