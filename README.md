# FLAC2iTunes
A simple tool to sync a directory of FLAC music to iTunes for the purpose of syncing with iDevices

## What this tool is for
By default, iTunes does not include support for FLAC, although it supports the Apple-created ALAC (Apple Lossless Audio Codec) format. This makes it tedious to convert FLAC to a format that iTunes can understand. FLAC2iTunes looks to solve this issue.

## How it works
By setting the `Source` and `Destination` in `App.config`, FLAC2iTunes will take any music that exists in the source directory, convert it if needed to ALAC, and move it to the same location in the destination directory. This is done by saving the location of the source file and the CRC32 hash of the first 512 bytes of each music file as metadata into the tag metadata of the converted or copied file dedicated for iTunes.

FLAC2iTunes utilizes the iTunes Windows COM API and therefore only supports Windows.

## Why this is needed
I personally maintain my music library by curating the source files. Any additions or changes in metadata is done in the source files. I needed an easy way to be able to sync to my iPod Classic and iPhone that was essentially hands-free.

## Features

* Automatically syncs/updates the iTunes database
* FLAC is converted to 16 bit 44.1khz for compatibility with iDevices
* Files are fingerprinted so changed and new files will be automatically synced
* Multithreaded!

## Known limitations/issues

* OGG is not supported at this time
* No intuitive progress indicator is currently shown
