# SRTSpliter

为了制作TTS训练集写的自用小工具，用SRT的标注，把一段长音频分割成一堆短音频

A tiny tool to split wav file into segments using SRT annotation.

Self-used tool for TTS training dataset making. 

# Requirements

- FFMPEG (add Enviroment variable on Windows)
- .NET 8 runtime.

# Usage
make sure name of wav file align with srt file.

```
├─data
│      1.srt
│      1.wav
│      2.srt
│      2.wav
│      3.srt
│      3.wav
|      ...
```

command

```./SRTSpliter.exe <path/to/1.srt> <path/to/2.srt> <path/to/3.srt> <path/to/4.srt> ....```

