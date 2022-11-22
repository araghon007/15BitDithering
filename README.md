# 15-Bit Dithering
Floyd-Steinberg dithering for quantizing 24-bit PNGs down to 15 bits.

## Why?
I was messing around with devkitPro and libnds, trying to make something for the DS, and couldn't find a way to dither 24-bit images, so here we are.

## Usage
Input PNGs need to be 24-bit!

Drag your PNG(s) (or folders) onto the executable and Windows will do its magic, adding the file paths as arguments. 

Or if you feel old-timey, you can do:

    15BitDithering.exe input.png [input2.png ...]
