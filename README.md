# BedrockServer2000

A linux command line Minecraft Bedrock server wrapper (code can easily be modified to run on Windows)

_Some part of the source code were taken from my other repository (<https://github.com/BaoUida2000/minecraft-bedrock-server-manager>) which was forked from Benjerman's Minecraft Server Manager (<https://github.com/Benjerman/Minecraft-Server-Manager>)._

## Construction in progress

This software is still in development process, some features haven't been implemented and there are lots of bugs so don't expect it to work perfectly.

## Features

### Current features

- Server stop message to let players know that the server will close in 10 seconds
- Automated backup saving, loading and other automated functions (has major bugs)
- Server exit timeout (automatically kill the server process if its exit procedure takes more than 30 seconds)
- Configuration file
- Ban list (has minor bugs)

### Unimplemented features

- Automatic backup on DateTIme
- Logging
- Chat logging
- Better exception handlers

## Known bugs

- Exception thrown when ban list file is not found.
- Automatic backup function sometimes causes some items or entities in the game to get deleted.

## Screenshots

![app_screenshot](resources/screenshots/app_screenshot.png)
