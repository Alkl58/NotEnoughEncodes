# NotEnoughEncodes

NotEnoughEncodes is a small GUI Handler for aomenc (AV1). 

NotEnoughEncodes is a tool to make encoding easier and faster for the AV1 reference encoder.

It splits the Source Video into multiple chunks and encode them parallel with the same given settings. 
At the end it will [Concatenate](https://trac.ffmpeg.org/wiki/Concatenate) the chunks into a single video (no audio!).

This tool is Windows only. For multiplatform and more features check out the CLI Tool [Av1an](https://github.com/master-of-zen/Av1an).

![alt text](https://i.imgur.com/8BOlXP9.png)

---

[![Build status](https://ci.appveyor.com/api/projects/status/ku2rkeo5u4mm164l/branch/master?svg=true)](https://ci.appveyor.com/project/Alkl/notenoughencodes/branch/master)

### Installation:

1. Download [ffmpeg](https://www.ffmpeg.org/download.html), [aomenc](https://ci.appveyor.com/project/marcomsousa/build-aom/history) and [NotEnoughEncodes](https://github.com/Alkl58/NotEnoughEncodes/releases).
2. Create a new Folder and put all .exe files in them (ffmpeg, aomenc and NotEnoughEncodes.exe)

### Usage:
1. Open NotEnoughEncodes.exe
2. Select your video file
3. Select the Chunk length in seconds. (The Chunks won't be exactly this long. ffmpeg will cut it at the nearest Key-Frame!)
4. Edit the Encoding settings. (Constant Quality Mode is the only mode supported yet!)
5. Click on "Start Encode". During the encoding process the programm will look like frozen. If you want to cancel the process you have to go into the Taskmanager and kill it by yourself.

---
### How does this program work?:
This programm will split the given video file into roughly equally chunks. This splitting process is not reencoding the video.
After splitting it will encode the chunks with x amount of workers. When finished, it will mux the encoded files together in one .mkv file.
You have to add Audio seperatly. 

### Disclaimer:
- This is my first Project. I don't really know much about programming and github.

