using FLAC2iTunes.Models;
using FLAC2iTunes.Models.Data;
using FLAC2iTunes.Models.Data.iTunes;
using FLAC2iTunes.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FLAC2iTunes
{
    public class ConversionService
    {
        private string Source { get; set; }
        private string Destination { get; set; }
        private LocalLibraryService LocalLibraryService { get; set; }
        private iTunesService iTunesService { get; set; }
        private List<Track> iTunesTracks { get; set; }
        public List<TrackFingerprint> LocalHashes { get; set; }
        public List<TrackFingerprint> iTunesFingerprints { get; set; }

        BlockingCollection<string> UnsupportedQueue = new BlockingCollection<string>();
        BlockingCollection<string> SupportedQueue = new BlockingCollection<string>();

        public ConversionService()
        {
            Source = ConfigurationManager.AppSettings["Source"];
            Destination = ConfigurationManager.AppSettings["Destination"];
            LocalLibraryService = new LocalLibraryService();
            iTunesService = new iTunesService();
        }

        public void Init()
        {
            iTunesTracks = iTunesService.GetAllTracks().ToList();
            iTunesFingerprints = new List<TrackFingerprint>();
            LocalHashes = new List<TrackFingerprint>();

            foreach (var serializedFingerprint in iTunesTracks.Select(it => it.Comment))
            {
                try
                {
                    iTunesFingerprints.Add(new TrackFingerprint(serializedFingerprint));
                } catch (Exception)
                {

                }
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

            return converter.OutputPath;
        }

        public void CleanLibrary()
        {
            var hashes = LocalHashes.Select(lh => lh.Hash);

            var orphanedLibraryItems = iTunesTracks.Where(it => !hashes.Contains(it.Comment));

            foreach (var item in orphanedLibraryItems)
            {
                File.Delete(item.Location);
                iTunesService.RemoveFile(item.Location);
            }
        }

        public void ProcessSupportedMusic()
        {
            int threadCount = 8;
            Task[] workers = new Task[threadCount];

            for (int i = 0; i < threadCount; i++)
            {
                int workerId = i;
                Task task = new Task(() => ProcessSupportedFile(workerId));
                workers[i] = task;

                task.Start();
            }

            var supportedFile = LocalLibraryService.GetSupportedMusicFilePaths();

            foreach (var file in supportedFile)
            {
                SupportedQueue.Add(file);
            }

            SupportedQueue.CompleteAdding();
            Task.WaitAll(workers);

            Console.WriteLine("Done processing unsupporteds");
            Console.ReadLine();
        }

        private void ProcessSupportedFile(int workerId)
        {
            Console.WriteLine("Worker {0} is starting", workerId);

            foreach (var file in SupportedQueue.GetConsumingEnumerable())
            {
                Console.WriteLine("Worker {0} is checking the file {1}", workerId, file);
                Console.WriteLine("Calculating file size for " + file);

                var fileSize = Helpers.GetFileSize(file);

                bool existsInLibrary = iTunesFingerprints.Any(f => f.File == file && f.FileSize == fileSize);

                if (!existsInLibrary)
                {
                    Console.WriteLine("File does not exist in the library and does not need conversion. Copying...");
                    var crc32 = Helpers.GetFileCRC32(file);

                    var fingerprint = new TrackFingerprint(file, crc32, fileSize);
                    var output = CopySupportedFile(file, fingerprint);

                    iTunesService.AddFile(output);
                    LocalHashes.Add(fingerprint);
                }
                else
                {
                    Console.WriteLine("File already exists in the library!");
                }
            }

            Console.WriteLine("Worker {0} is stopping", workerId);
        }

        public void ProcessUnsupportedMusic()
        {
            int threadCount = 8;
            Task[] workers = new Task[threadCount];

            for (int i = 0; i < threadCount; i++)
            {
                int workerId = i;
                Task task = new Task(() => ProcessUnsupportedFile(workerId));
                workers[i] = task;

                task.Start();
            }

            var unsupportedFile = LocalLibraryService.GetUnsupportedMusicFilePaths();

            foreach (var file in unsupportedFile)
            {
                UnsupportedQueue.Add(file);
            }

            UnsupportedQueue.CompleteAdding();
            Task.WaitAll(workers);

            Console.WriteLine("Done processing unsupporteds");
            Console.ReadLine();
        }

        public void ProcessUnsupportedFile(int workerId)
        {
            Console.WriteLine("Worker {0} is starting", workerId);

            foreach (var file in UnsupportedQueue.GetConsumingEnumerable())
            {
                Console.WriteLine("Worker {0} is checking the file {1}", workerId, file);
                Console.WriteLine("Calculating file size for " + file);

                var fileSize = Helpers.GetFileSize(file);

                bool existsInLibrary = iTunesFingerprints.Any(f => f.File == file && f.FileSize == fileSize);

                if (!existsInLibrary)
                {
                    Console.WriteLine("File does not exist in the library and needs to be converted. Converting...");
                    var crc32 = Helpers.GetFileCRC32(file);

                    var fingerprint = new TrackFingerprint(file, crc32, fileSize);

                    var convertedFile = ConvertALACToFLAC(file, fingerprint);

                    iTunesService.AddFile(convertedFile);
                    LocalHashes.Add(fingerprint);
                }
                else
                {
                    Console.WriteLine("File already exists in the library!");
                }
            }

            Console.WriteLine("Worker {0} is stopping", workerId);
        }
    }
}
