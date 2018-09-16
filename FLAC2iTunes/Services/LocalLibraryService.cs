using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FLAC2iTunes.Services
{
    class LocalLibraryService
    {
        private string Source { get; set; }
        private string Destination { get; set; }
        private string[] Extensions { get; set; }
        private string[] UnsupportedExtensions { get; set; }
        private string[] SupportedExtensions { get; set; }

        public LocalLibraryService()
        {
            Source = ConfigurationManager.AppSettings["Source"];
            Destination = ConfigurationManager.AppSettings["Destination"];
            SupportedExtensions = ConfigurationManager.AppSettings["SupportedFileExtensions"].Split('|');
            UnsupportedExtensions = ConfigurationManager.AppSettings["UnsupportedFileExtensions"].Split('|');
        }

        public IEnumerable<string> GetAllMusicFilePaths()
        {
            var files = Directory.EnumerateFiles(Source, "*.*", SearchOption.AllDirectories)
                .Where(s => 
                    SupportedExtensions.Concat(UnsupportedExtensions)
                        .Contains(Path.GetExtension(s).Replace(".", ""))
                );

            return files;
        }

        public IEnumerable<string> GetSupportedMusicFilePaths()
        {
            var files = Directory.EnumerateFiles(Source, "*.*", SearchOption.AllDirectories)
                .Where(s => SupportedExtensions.Contains(Path.GetExtension(s).Replace(".", "")));

            return files;
        }

        public IEnumerable<string> GetUnsupportedMusicFilePaths()
        {
            var files = Directory.EnumerateFiles(Source, "*.*", SearchOption.AllDirectories)
                .Where(s => UnsupportedExtensions.Contains(Path.GetExtension(s).Replace(".", "")));

            return files;
        }
    }
}
