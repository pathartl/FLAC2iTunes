# FLAC2iTunes
A simple tool to sync a directory of FLAC music to iTunes for the purpose of syncing with iDevices

## What this tool is for
By default, iTunes does not include support for FLAC, although it supports the Apple-created ALAC (Apple Lossless Audio Codec) format. This makes it tedious to convert FLAC to a format that iTunes can understand. FLAC2iTunes looks to solve this issue.

## How it works
By setting the `Source` and `Destination` in `App.config`, FLAC2iTunes will take any music that exists in the source directory, convert it if needed to ALAC, and move it to the same location in the destination directory. It does this by:

1. Checks if the file extension is supported (dictated by the `App.config` keys `SupportedFileExtensions` and `UnsupportedFileExtensions`
1. If the file is NOT supported
   1. It then creates a fingerprint of the source file and then looks for any track in the iTunes database with the `Comment` field set to the hash.
   1. If there is not a track with the applicable fingerprint, FLAC2iTunes converts the source file to ALAC and sets the `Comment` metadata field to the fingerprint of the source file
1. If the file IS supported
   1. It calculates a fingerprint of the source file and then looks for any track in the iTunes database with the `Comment` field set to the fingerprint.
   1. If there is not a track with the applicable fingerprint, FLAC2iTunes copies the source file to the destination directory maintaining the directory structure.
1. The iTunes database is then checked to see if there are any library files that do not have a file in the destination directory. It removes this from the library to avoid potential duplicates or orphaned files.

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
* The application may or may not remove podcasts or videos from the library. This is untested.
* Fingerprinting is currently only done by file path and file size. These are generally unique enough for these types of files.
* Multithreading is set at a hard 8 threads. This will be automatic/configurable in the future
* No intuitive progress indicator is currently shown
