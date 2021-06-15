# BedrockServer2000

A linux command line Minecraft Bedrock server wrapper (code can easily be modified to run on Windows)

_Some part of the source code were taken from my other repository (<https://github.com/BaoUida2000/minecraft-bedrock-server-manager>) which was forked from Benjerman's Minecraft Server Manager (<https://github.com/Benjerman/Minecraft-Server-Manager>)._

## Construction in progress

This software is still in development process, some features haven't been implemented and there are lots of bugs so don't expect it to work perfectly.

## Features

### Current features

- Server stop message to let players know that the server will close in 10 seconds
- Automated backup saving, loading and other automated functions
- Server exit timeout (automatically kill the server process if its exit procedure takes more than 30 seconds)
- Configuration file
- Color-highlighted output in terminal

### Unimplemented features

- Sort backup list by folder names (D_M_Y-H_M_S)
- Automatic backup on DateTIme
- Logging
- Chat input and logging (depends on server software, which hasn't implement that)
- Better exception handlers

## Known bugs

No bugs were found since the last test.

## Screenshots

![app_screenshot](app_screenshot.png)
