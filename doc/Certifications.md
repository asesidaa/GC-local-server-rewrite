# Run on Arcade Machine

Since the arcade is using some version XP, which does not support SHA256, the generated certification will not work.

To solve this issue, you have to generate a certification using weaker signature algorithm.

For certificate generation, I am using https://certificatetools.com/

## Generate Root CA

First generate a root CA, by choosing the Root Certificate Authority. 

Under Subject Attributes, set Common Name to "Taito Arcade Machine CA", others are optional and can have any value.

Under CSR options, choose MD5 as hash algorithm, choose Self-Sign type, then input year number. 

Submit and download "PKCS#12 Certificate and Key".

## Generate Server Certificate

Then generate server certificate, by choose Web Server as template.

Under Subject Attributes, change Common Name to "GC local server"

Under Subject Alternative Names, add DNS: cert.nesys.jp,data.nesys.jp,nesys.taito.co.jp,fjm170920zero.nesica.net

Under CSR options, choose MD5 as hash algorithm, choose Sign With Certificate Authority 0 (the one just generated), then input year number. 

Submit and download "PKCS#12 Certificate and Key".

## Import to server side

In server side, import the certificates using mmc.exe. You can find a detailed guide at https://www.thesslstore.com/knowledgebase/ssl-install/how-to-import-intermediate-root-certificates-using-mmc/

Before import, first delete any old certificate Named "Taito Arcade Machine CA" or "GC local server" under Personal and "Trusted Root Certification Authorities"

The root certificate goes to "Personal" and "Trusted Root Certification Authorities"

The server certificate goes to "Personal"

## Import to game side

On the machine, first download and install  http://outwardtruth.com/tools/win2k3tools/win2k3resourcetoolkit.htm 

After that, you will get WinHttpCertCfg.exe in "C:\Program Files\Windows Resource Kits\Tools". Use the following cmd to import root certificate

```
WinHttpCertCfg -i "E:\0_GC_LOCAL_SERVER\root.pfx" -C LOCAL_MACHINE\Root -a SYSTEM
```

After that, using IE to install the 2 certificates to MY(Personal) tab

## Config the server

In server, the config file is GC-local-server-rewrite.exe.config

There, you will find an XML entry 

```xml
<add key="ServerIp" value="127.0.0.1"/>
```

Change value to the real server Ip

Currently, there are two DB files, music.db3 and music4MAX.db3, which correspond to 4.52 and 4.61 data, change the following field according to your game version.

```xml
<add key="MusicDBName" value="music.db3"/>
```



## Config host file

In game side, change the host file, add the following entries

```
127.0.0.1 cert.nesys.jp
127.0.0.1 data.nesys.jp
127.0.0.1 nesys.taito.co.jp
127.0.0.1 fjm170920zero.nesica.net
```

You can now try to boot the game, it should be able to connect to the server.

## Troubleshooting

Import following to registry

```
Windows Registry Editor Version 5.00

[HKEY_LOCAL_MACHINE\SOFTWARE\TAITO]
"DisableLocalServer"=dword:00000001

[HKEY_LOCAL_MACHINE\SOFTWARE\TAITO\TYPEX]
"UpdateStep"=dword:00000000
"LogLevel"=dword:00000003
"TrafficCount"=dword:00000002
"ConditionTime"=dword:0000012c
"EventNextTime"=dword:00000384
"LogPath"="D:\\\\system\\\\CmdFile\\\\log"
"NewsPath"="D:\\\\system\\\\DUA\\\\news"
"EventPath"="D:\\\\system\\\\DUA\\\\event"
"Resolution"=dword:00000000
"ScreenVertical"=dword:00000000
"EventModeEnable"=dword:00000000
"CoinCredit"=dword:00000001
"UserSelectEnable"=dword:00000000
"GameResult"=dword:00000000
"IOErrorCoin"=dword:00000000
"IOErrorCredit"=dword:00000000
"GameKind"=dword:0004A2B9
```

Then boot the game, log files should be under ``D:\system\CmdFile\log``

Open the one named `access*.log`, where * is the date.

You can find the communication log, if you see any errors other than 404, report at https://github.com/asesidaa/GC-local-server-rewrite or contact me @asesidaa in discord.

Some common errors:

0x00002F8F: Certificate error, either the root certificate is not imported (so not trusted), or the certificate is not correct/not recognized by XP

0x00002F9A: No private key, check if your certificate file is imported with private key.

# DAT files

Under game folder data/boot, there are all the dat files.

If you have a new version, contact @Javaguru to use his template to parse these, and send me the parsed data.

For music db, ask for a parsed json file for the new stage_param.dat. I can import these and make a new db file for the new version.