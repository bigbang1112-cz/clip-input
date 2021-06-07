# Clip Input

[![GitHub release (latest by date including pre-releases)](https://img.shields.io/github/v/release/BigBang1112-cz/clip-input?include_prereleases&style=for-the-badge)](https://github.com/BigBang1112-cz/clip-input/releases)
[![GitHub all releases](https://img.shields.io/github/downloads/BigBang1112-cz/clip-input/total?style=for-the-badge)](https://github.com/BigBang1112-cz/clip-input/releases)

Powered by [GBX.NET](https://github.com/BigBang1112/gbx-net).

**Clip Input** is a tool that can extract inputs from replays **and create a MediaTracker visualization as .Clip.Gbx** which can be then imported next to the replay.

The tool currently works on drag and dropping. You can also drag and drop multiple files to process.

A web interface is planned in the future.

Possible uses of Clip Input:
- Inspect other player's solutions
- Inspect very old replays reaching 2003
- Input visualization for map GPS
- Save time rendering the input visualization

Advantages of Clip Input over other input inspection methods:
- No video rendering is required to showcase the inputs
- Compatibility of replays across the whole Trackmania franchise
- Currently it's the fastest method to check inputs in Trackmania 2
- Inputs can be rendered together with the clip

Currently the tool can read inputs from these files:
- Replay.Gbx
- Ghost.Gbx
- Clip.Gbx (that contains minimally 1 ghost)

Where can I import the outputted Clip.Gbx?
- For replays as old as TM1.0-TMUF, it is recommended to import the clip in the latest version of Trackmania United Forever.
- For replays from TM2, it is recommended to import the clip in the latest version of ManiaPlanet.

## Trackmania 2020

This version doesn't have inputs available in replays like other Trackmania games. They might be even encrypted, making this likely an impossible thing to solve.

## Trackmania Turbo

This version has inputs available. Somehow keyboard inputs are stored as analog values, but that's not the main problem. MediaTracker in Trackmania Turbo doesn't support any kind of 2D rendering, including images and triangles, which means there isn't a way to show the inputs.

## Troubleshooting

- **Issue: The program blinks when trying to run**
  - Solution: Install [.NET 5 Runtime](https://dotnet.microsoft.com/download/dotnet/5.0/runtime) and choose x64 or x86 depending on the OS you use.
- Issue: The analog input is white instead of blue
  - Solution: Analog input is made out of 2D triangles which become always white on PC2 shader quality. To fix this, **set the Shader Quality to at least PC3 Low**.

## Settings

You can manage the default values in `Config.yml` file.

### Command line arguments

The first argument always must be the file to process. You can also put multiple files, but at the moment, you can't use optional arguments when processing multiple files at once.

```
ClipInputCLI [file_name] [optional_args]
```

Optional arguments:

| Argument name | Default value | Value format | Description
| --- | --- | --- | ---
| -ratio, -aspectRatio | 16:9 | \[x\]:\[y\] | Ratio for images and triangles to correctly resize for the render settings
| -scale | 0.5 | \[scale\] or \[scaleX\],\[scaleY\] | Scale of the whole visualizer
| -space, -spacing | 1.05 | \[space\] or \[spaceX\],\[spaceY\] | Breathing space for each element
| -pos, -position | 0,0.5 | \[x\],\[y\] | Position of the whole visualizer from center
| -showAfterInteraction | false | \[true/false\] | Determines if each input should appear only after being used
| -padOffset | 0.385,0 | \[x],\[y\] | Tells how far from each other should the base pads be
| -padColor | 0.11,0.44,0.69,1 | \[r],\[g\],\[b\],\[a\] | Color of the pad
| -padBrakeColor | 0.69,0.18,0.11,1 | \[r],\[g\],\[b\],\[a\] | Color of the analog brake
| -padBackgroundColor | 0.3,0.3,0.3,0.3 | \[r\],\[g\],\[b\],\[a\] | Background color of the pad
| -padStartPos, -padStartPosition | 0.16,-0.45,0 | \[x\],\[y\],\[z\] | Starting symmetric point of the triangle
| -padEndPos -padEndPosition | 0.6,0,0 | \[x\],\[y\],\[z\] | Ending meeting point of the triangle
| -theme | black | \[black/white\] | Visual style of the input visualization
| -start, -startOffset | 0 | \[seconds\] | Delay in seconds before the whole visualizer starts