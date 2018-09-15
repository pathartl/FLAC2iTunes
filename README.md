# FLAC2iTunes
A simple tool to sync a directory of FLAC music to iTunes

## What this tool is for
By default, iTunes does not include support for FLAC, although it supports the Apple-created ALAC (Apple Lossless Audio Codec) format. This makes it tedious to convert FLAC to a format that iTunes can understand. FLAC2iTunes looks to solve this issue.

## How it works
By setting the `Source` and `Destination` in `App.config`, FLAC2iTunes will take any music that exists in the source directory, convert it if needed to ALAC, and move it to the same location in the destination directory. It does this by:

1. Checks if the file extension is supported (dictated by the `App.config` keys `SupportedFileExtensions` and `UnsupportedFileExtensions`
1. If the file is NOT supported
   1. It then calculates a CRC32 hash of the source file and then looks for any track in the iTunes database with the `Comment` field set to the hash.
   1. If there is not a track with the applicable hash, FLAC2iTunes converts the source file to ALAC and sets the `Comment` metadata field to the CRC32 hash of the source file
1. If the file IS supported
   1. It calculates a CRC32 hash of the source file and then looks for any track in the iTunes database with the `Comment` field set to the hash.
   1. If there is not a track with the applicable hash, FLAC2iTunes copies the source file to the destination directory maintaining the directory structure.
1. The iTunes database is then checked to see if there are any library files that do not have a file in the destination directory. It removes this from the library to avoid potential duplicates or orphaned files.

FLAC2iTunes utilizes the iTunes Windows COM API and therefore only supports Windows.

## Why this is needed
I personally maintain my music library by curating the source files. Any additions or changes in metadata is done in the source files. I needed an easy way to be able to sync to my iPod Classic and iPhone that was essentially hands-free.

## Known limitations/issues

* OGG is not supported at this time
* The application may or may not remove podcasts or videos from the library. This is untested.
* Syncs can be fairly slow depending on the music library. In my 200GB library, it took about three hours for a full conversion and sync. The sync is currently single threaded, but there are plans to make it multithreaded.
* RAM usage can be fairly high. Depending on library size it can be anywhere from 300MB to 1.5GB of memory usage.
* The conversion process will create files on your local disk and will eat up disk space even if you're using a network share. This may be due to some way in which FFmpeg converts the music on Windows.
