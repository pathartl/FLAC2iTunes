using FLAC2iTunes.Models;
using FLAC2iTunes.Models.Data;
using FLAC2iTunes.Models.Data.iTunes;
using FLAC2iTunes.Services;
using iTunesLib;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
        public List<LocalHash> LocalHashes { get; set; }

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
            LocalHashes = new List<LocalHash>();
        }

        public string ConvertALACToFLAC(string filePath, string uniqueHash = "")
        {
            var output = filePath.Replace(Source, Destination);
            var outputPath = Path.GetDirectoryName(output);
            var basename = Path.GetFileNameWithoutExtension(output);

            Directory.CreateDirectory(outputPath);

            var converter = new FFmpegCommand();
            converter.Codec = "alac";
            converter.InputPath = filePath;
            converter.OutputPath = String.Format("{0}\\{1}.m4a", outputPath, basename);
            converter.Hash = uniqueHash;

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
            var supportedFile = LocalLibraryService.GetSupportedMusicFilePaths();

            foreach (var file in supportedFile)
            {
                ProcessSupportedFile(file);
            }
        }

        private void ProcessSupportedFile(string file)
        {
            Console.WriteLine("Calculating hash for " + file);
            var crc32 = Helpers.GetFileCRC32(file);

            bool existsInLibrary = iTunesTracks.Any(t => t.Comment == crc32);

            if (!existsInLibrary)
            {
                Console.WriteLine("File does not exist in the library and does not need conversion. Copying...");
                var output = file.Replace(Source, Destination);
                var outputPath = Path.GetDirectoryName(output);

                Directory.CreateDirectory(outputPath);
                File.Copy(file, output, true);
                LocalHashes.Add(new LocalHash(file, crc32));

                iTunesService.AddFile(output);
            } else
            {
                Console.WriteLine("File already exists in the library!");
            }
        }

        public void ProcessUnsupportedMusic()
        {
            var unsupportedFile = LocalLibraryService.GetUnsupportedMusicFilePaths();

            foreach (var file in unsupportedFile)
            {
                ProcessUnsupportedFile(file);
            }
        }

        private void ProcessUnsupportedFile(string file)
        {
            Console.WriteLine("Calculating hash for " + file);
            var crc32 = Helpers.GetFileCRC32(file);

            bool existsInLibrary = iTunesTracks.Any(t => t.Comment == crc32);

            if (!existsInLibrary)
            {
                Console.WriteLine("File does not exist in the library and needs to be converted. Converting...");
                var convertedFile = ConvertALACToFLAC(file, crc32);

                iTunesService.AddFile(convertedFile);
                LocalHashes.Add(new LocalHash(file, crc32));
            } else
            {
                Console.WriteLine("File already exists in the library!");
            }
        }

        private void ProcessUnsupportedCallback(Object file)
        {
            Console.WriteLine(String.Format("Thread opened for file {0}", file.ToString()));

            var crc32 = Helpers.GetFileCRC32(file.ToString());

            bool existsInLibrary = iTunesTracks.Any(t => t.Comment == crc32);

            if (!existsInLibrary)
            {
                var convertedFile = ConvertALACToFLAC(file.ToString(), crc32);

                iTunesService.AddFile(file.ToString());
            }

            Console.WriteLine(String.Format("Thread closed for file {0}", file.ToString()));
        }
    }
}
