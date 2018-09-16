using FLAC2iTunes.Models.Data.iTunes;
using iTunesLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FLAC2iTunes
{
    public class iTunesService
    {
        private iTunesAppClass iTunes { get; set; }
        private IITLibraryPlaylist Library { get; set; }
        private IITTrackCollection Tracks { get; set; }

        public iTunesService()
        {
            iTunes = new iTunesAppClass();
            Library = iTunes.LibraryPlaylist;
            Tracks = Library.Tracks;
        }

        public IEnumerable<Track> GetAllTracks()
        {
            List<Track> tracks = new List<Track>();

            foreach (var track in Tracks)
            {
                tracks.Add(new Track(track));
            }

            return tracks;
        }

        public void AddFile(string path)
        {
            Library.AddFile(path.Replace("/", "\\"));
            Console.WriteLine(String.Format("Adding {0} to the library", path));
        }

        public void RemoveFile(string path)
        {
            foreach (dynamic track in Tracks)
            {
                if (track.Location == path)
                {
                    track.Delete();
                }
            }
        }
    }
}
