# BedrockServer2000

A linux command line Minecraft Bedrock server wrapper

_Some part of the source code were taken from my other repository (<https://github.com/BaoUida2000/minecraft-bedrock-server-manager>) which was forked from Benjerman's Minecraft Server Manager (<https://github.com/Benjerman/Minecraft-Server-Manager>)._

## Construction in progress

This software is still in development process and some features haven't been implemented so don't expect it to work perfectly.

## Features

### Current features

- Auto start server
- Server closing message to let players know that the server will close in 10 seconds
- Automated backup saving
- Automated backup loading
- Configuration file

### Unimplemented features

- Sort backup list by folder names (D_M_Y-H_M_S)
- Automatic backup on DateTIme
- Logging
- Chat input and logging (depends on server software, which hasn't implement that)

## Known bugs

- backupLimit doesn't update instantly after using the `set` command

## Screenshots

![app_screenshot](app_screenshot.png)
