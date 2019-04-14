using FLAC2iTunes.Models;
using FLAC2iTunes.Models.Data;
using FLAC2iTunes.Models.Data.iTunes;
using FLAC2iTunes.Services;
using iTunesLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FLAC2iTunes
{
    public enum FileChangedState
    {
        NotChanged,
        Removed,
        Modified
    }

    public class ConversionService
    {
        private string Source { get; set; }
        private string Destination { get; set; }
        private LocalLibraryService LocalLibraryService { get; set; }
        private iTunesService iTunesService { get; set; }
        private List<Track> iTunesTracks { get; set; }
        public List<TrackFingerprint> LocalHashes { get; set; }
        public Dictionary<int, string> ThreadMessages { get; set; }
        public Dictionary<string, TrackFingerprint> iTunesFingerprintsMap { get; set; }
        public int Threads { get; set; }

        private string[] UnsupportedExtensions { get; set; }
        private string[] SupportedExtensions { get; set; }

        public List<string> AddedQueue = new List<string>();
        public List<string> UpdatedQueue = new List<string>();
        public List<string> RemovedQueue = new List<string>();

        BlockingCollection<string> AddedThreadQueue = new BlockingCollection<string>();
        BlockingCollection<string> RemovedThreadQueue = new BlockingCollection<string>();
        BlockingCollection<Track> UpdatedThreadQueue = new BlockingCollection<Track>();

        public List<string> iTunesAddQueue = new List<string>();
        public List<string> iTunesRemoveQueue = new List<string>();
        public List<string> iTunesUpdateQueue = new List<string>();

        public List<Track> TracksToRemoveFromLibrary = new List<Track>();
        public List<string> TracksToAddToLibrary = new List<string>();
        public List<Track> TracksToUpdateInLibrary = new List<Track>();

        private BlockingCollection<string> CheckForChangesQueue = new BlockingCollection<string>();

        public ConversionService()
        {
            Source = ConfigurationManager.AppSettings["Source"];
            Destination = ConfigurationManager.AppSettings["Destination"];
            LocalLibraryService = new LocalLibraryService();
            iTunesService = new iTunesService();
            ThreadMessages = new Dictionary<int, string>();

            SupportedExtensions = ConfigurationManager.AppSettings["SupportedFileExtensions"].Split('|');
            UnsupportedExtensions = ConfigurationManager.AppSettings["UnsupportedFileExtensions"].Split('|');
        }

        public void Init()
        {
            iTunesService.Init();

            if (Threads <= 0)
            {
                Threads = 8;
            }
        }

        public string ConvertALACToFLAC(string filePath, TrackFingerprint fingerprint)
        {
            var output = filePath.Replace(Source, Destination);
            var outputPath = Path.GetDirectoryName(output);
            var basename = Path.GetFileNameWithoutExtension(output);

            Directory.CreateDirectory(outputPath);

            var converter = new FFmpegCommand();
            converter.Codec = "alac";
            converter.InputPath = filePath;
            converter.OutputPath = String.Format("{0}\\{1}.m4a", outputPath, basename);
            converter.Hash = fingerprint.ToString();

            converter.Convert();

            return converter.OutputPath;
        }

        public string CopySupportedFile(string filePath, TrackFingerprint fingerprint)
        {
            FFmpegCommand converter = new FFmpegCommand();
            converter.Codec = "copy";
            converter.InputPath = filePath;
            converter.OutputPath = filePath.Replace(Source, Destination);

            var outputPath = Path.GetDirectoryName(converter.OutputPath);
            Directory.CreateDirectory(outputPath);

            converter.Convert();

            var file = TagLib.File.Create(converter.OutputPath);
            file.Tag.Comment = fingerprint.ToString();
            file.Save();
            file.Dispose();
            

            return converter.OutputPath;
        }

        public void CleanLibrary()
        {
            var hashes = LocalHashes.Select(lh => lh.Hash);

            var orphanedLibraryItems = iTunesTracks.Where(it => !hashes.Contains(it.Comment));

            foreach (var item in orphanedLibraryItems)
            {
                File.Delete(item.Location);
               // iTunesService.RemoveFile(item.Location);
            }
        }

        public void ScanForChanges()
        {
            var localFiles = LocalLibraryService.GetAllMusicFilePaths().ToList();

            var libraryLocalFileReferences = iTunesService.iTunesTracks.Where(itt => itt.Fingerprint != null).Select(itt => itt.Fingerprint.File).ToList();
            var localFilesNotInLibrary = localFiles.Where(lf => !libraryLocalFileReferences.Contains(lf)).ToList();
            var libraryFilesNotFoundInLocal = libraryLocalFileReferences.Where(llfr => !localFiles.Contains(llfr)).ToList();
            var filesFoundLocalAndInLibrary = localFiles.Where(lf => libraryLocalFileReferences.Contains(lf)).ToList();

            TracksToAddToLibrary.AddRange(localFilesNotInLibrary);
            TracksToRemoveFromLibrary.AddRange(iTunesService.iTunesTracks.Where(itt => itt.Fingerprint != null && libraryFilesNotFoundInLocal.Contains(itt.Fingerprint.File)));

            // Hydrates TracksToUpdateInLibrary
            // This is multithreaded as calculating hashes can take a while
            Task.WaitAll(CheckForChangedFiles(filesFoundLocalAndInLibrary));

            // Next, update/encode changed files. This is multithreaded as
            // encoding to Apple lossless may occur.
            Task.WaitAll(UpdateChangedFiles());

            // We have to remove tracks first in descending order
            // The iTunes database goes by indexes/total song counts
            // If we add or remove to that in any descending order,
            // all of our cached songs with their indexes are then invalidated
            TracksToRemoveFromLibrary = TracksToRemoveFromLibrary.OrderByDescending(t => t.iTunesTrack.Index).ToList();
            iTunesService.RemoveFiles(TracksToRemoveFromLibrary.Select(t => t.Location).ToList());

            // Next, add all remaining files. This is multithreaded as
            // encoding to Apple lossless may occur.
            Task.WaitAll(AddChangedFiles());
        }

        private Task[] UpdateChangedFiles()
        {
            Task[] workers = new Task[Threads];

            for (int i = 0; i < Threads; i++)
            {
                int workerId = i;
                Task task = new Task(() => UpdateChangedFile(workerId));
                workers[i] = task;

                task.Start();
            }

            foreach (var track in TracksToUpdateInLibrary)
            {
                UpdatedThreadQueue.Add(track);
            }

            UpdatedThreadQueue.CompleteAdding();

            return workers;
        }

        private void UpdateChangedFile(int workerId)
        {
            foreach (var track in UpdatedThreadQueue.GetConsumingEnumerable())
            {
                LogThread(workerId, $"Updating file {track.Location}");

                TracksToRemoveFromLibrary.Add(track);
                TracksToAddToLibrary.Add(track.Fingerprint.File);
            }
        }

        private Task[] AddChangedFiles()
        {
            Task[] workers = new Task[Threads];

            for (int i = 0; i < Threads; i++)
            {
                int workerId = i;
                Task task = new Task(() => AddChangedFile(workerId));
                workers[i] = task;

                task.Start();
            }

            foreach (var file in TracksToAddToLibrary)
            {
                AddedThreadQueue.Add(file);
            }

            AddedThreadQueue.CompleteAdding();

            return workers;
        }

        private void AddChangedFile(int workerId)
        {
            foreach (var file in AddedThreadQueue.GetConsumingEnumerable())
            {
                LogThread(workerId, $"Adding file {file}");

                var fileInfo = new FileInfo(file);
                var crc32 = Helpers.GetFileCRC32(file);
                var fileSize = Helpers.GetFileSize(file);

                var fingerprint = new TrackFingerprint(file, crc32, fileSize);

                if (SupportedExtensions.Contains(fileInfo.Extension.Substring(1, fileInfo.Extension.Length - 1)))
                {
                    LogThread(workerId, $"Copying added file {file}");

                    var copiedFile = CopySupportedFile(file, fingerprint);

                    iTunesService.AddFile(copiedFile);
                }
                else
                {
                    LogThread(workerId, $"Converting added file {file}");

                    var convertedFile = ConvertALACToFLAC(file, fingerprint);

                    iTunesService.AddFile(convertedFile);
                }
            }
        }

        private Task[] CheckForChangedFiles(IEnumerable<string> files)
        {
            Task[] workers = new Task[Threads];

            for (int i = 0; i < Threads; i++)
            {
                int workerId = i;
                Task task = new Task(() => CheckForChangedFile(workerId));
                workers[i] = task;

                task.Start();
            }

            foreach (var file in files)
            {
                CheckForChangesQueue.Add(file);
            }

            CheckForChangesQueue.CompleteAdding();

            return workers;
        }

        private void CheckForChangedFile(int workerId)
        {
            foreach (var file in CheckForChangesQueue.GetConsumingEnumerable())
            {
                LogThread(workerId, $"Checking for changes in {file}");

                var changed = false;

                //Console.WriteLine(file);
                var track = iTunesService.iTunesTracks.Where(itt => itt.Fingerprint != null && itt.Fingerprint.File == file).First();

                if (track.Fingerprint.FileSize != Helpers.GetFileSize(file))
                {
                    LogThread(workerId, $"File size has changed for {file}");
                    changed = true;
                }
                else
                {
                    if (track.Fingerprint.Hash != Helpers.GetFileCRC32(file))
                    {
                        LogThread(workerId, $"Hash has change for {file}");
                        changed = true;
                    }
                }

                if (changed)
                {
                    LogThread(workerId, $"File has changed: {file}");

                    lock (TracksToUpdateInLibrary)
                    {
                        TracksToUpdateInLibrary.Add(track);
                    }
                }
                else
                {
                    LogThread(workerId, $"File has not changed: {file}");
                }
            }
        }

        public Task[] ProcessUpdatedMusic()
        {
            // 4x as many threads because calculating hashes is FAST
            Task[] workers = new Task[Threads * 4];

            for (int i = 0; i < Threads * 4; i++)
            {
                int workerId = i;
                Task task = new Task(() => ProcessChangedFile(workerId));
                workers[i] = task;

                task.Start();
            }

            // Build full list of files
            var localFiles = LocalLibraryService.GetAllMusicFilePaths().ToList();
            var libraryFiles = iTunesService.iTunesFingerprints.Select(itf => itf.File).ToList();

            localFiles.AddRange(libraryFiles);
            localFiles = localFiles.Distinct().ToList();

            return workers;
        }

        public void ProcessChangedFile(int workerId)
        {
            LogThread(workerId, $"Worker {workerId} is starting");

            /*foreach (var file in ChangedQueue.GetConsumingEnumerable())
            {
                LogThread(workerId, $"Checking if exists for {file}");

                FileChangedState state = FileChangedState.NotChanged;

                if (!File.Exists(file))
                {
                    LogThread(workerId, "File does not exist in location. Remove it.");
                    state = FileChangedState.Removed;
                }
                else
                {
                    LogThread(workerId, "File exists");
                    var libraryTrack = iTunesFingerprints.Where(itf => itf.File == file).FirstOrDefault();

                    if (libraryTrack.Hash != Helpers.GetFileCRC32(file))
                    {
                        LogThread(workerId, "Hash has changed");
                        state = FileChangedState.Modified;
                    }
                }

                switch (state)
                {
                    case FileChangedState.Removed:
                        //iTunesService.RemoveFile(file);
                        break;

                    case FileChangedState.Modified:
                        //iTunesService.RemoveFile(file);
                        //iTunesService.AddFile(file);
                        break;
                }
            }*/
        }

        private void LogThread(int workerId, string message)
        {
            ThreadMessages[workerId] = message;
            Console.SetCursorPosition(0, workerId);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, workerId);

            if (message.Length > Console.WindowWidth)
            {
                Console.Write("\r" + message.Substring(0, Console.WindowWidth));
            }
            else
            {
                Console.Write("\r" + message);
            }
        }

    }
}
