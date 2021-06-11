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
- Automated backup loading **(needs improvement)**
- Configuration file **(has major bugs)**

### Unimplemented features

- Sort backup list by folder names (D_M_Y-H_M_S)
- Automatic backup on DateTIme
- Logging
- Chat input and logging (depends on server software, which hasn't implement that)

## Known bugs

- App.Config file is not found or embedded - into the program when do `dotnet publish`
- 'reload' doesn't show the updated config and need to restart the program after using the `set` command to fully update the configs
- Unknown crash where the app tells that the "embedded" configuration file is in a directory called /home/user/.net/

## Screenshots

![app_screenshot](app_screenshot.png)
