using FLAC2iTunes.Models.Data;
using FLAC2iTunes.Models.Data.iTunes;
using iTunesLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FLAC2iTunes
{
    public class iTunesService
    {
        private iTunesAppClass iTunes { get; set; }
        private IITLibraryPlaylist Library { get; set; }
        public IITTrackCollection Tracks { get; set; }
        public object LocalHash { get; private set; }
        public List<TrackFingerprint> iTunesFingerprints { get; set; }
        public List<Track> iTunesTracks { get; set; }
        private Dictionary<string, IITTrack> TrackFileMap { get; set; }
        private Dictionary<int, string> TrackIdFileMap { get; set; }

        public iTunesService()
        {
            iTunes = new iTunesAppClass();
            Library = iTunes.LibraryPlaylist;
            Tracks = Library.Tracks;
            TrackIdFileMap = new Dictionary<int, string>();
            iTunesFingerprints = new List<TrackFingerprint>();
            iTunesTracks = new List<Track>();
        }

        public void Init()
        {
            iTunesTracks.AddRange(GetAllTracks());

            foreach (var track in iTunesTracks)
            {
                try
                {
                    Console.WriteLine($"Parsing fingerprint from ${track.Location}");

                    var serializedFingerprint = track.Comment;
                    var fingerprint = new TrackFingerprint(serializedFingerprint);

                    iTunesFingerprints.Add(fingerprint);
                }
                catch { }
            }            
        }

        public IEnumerable<Track> GetAllTracks()
        {
            List<Track> tracks = new List<Track>();

            IITFileOrCDTrack currentTrack;
            int trackIndex = Tracks.Count;

            while (trackIndex != 0)
            {
                currentTrack = Tracks[trackIndex] as IITFileOrCDTrack;

                if (currentTrack != null && currentTrack.Kind == ITTrackKind.ITTrackKindFile)
                {
                    tracks.Add(new Track(currentTrack));
                }

                trackIndex--;
            }

            return tracks;
        }

        public void AddFile(string path)
        {
            Library.AddFile(path.Replace("/", "\\"));
        }

        public void RemoveFiles(IEnumerable<string> files)
        {
            IITFileOrCDTrack currentTrack;
            int trackIndex = Tracks.Count;
            int tracksRemoved = 0;

            if (files.Count() != 0)
            {
                while (trackIndex != 0 && tracksRemoved < Tracks.Count)
                {
                    currentTrack = Tracks[trackIndex] as IITFileOrCDTrack;

                    if (currentTrack != null && currentTrack.Kind == ITTrackKind.ITTrackKindFile)
                    {
                        if (currentTrack.Location != null && files.Contains(currentTrack.Location))
                        {
                            File.Delete(currentTrack.Location);
                            currentTrack.Delete();
                            tracksRemoved++;
                        }
                    }

                    trackIndex--;
                }
            }
        }

        public string GetDestinationFileBySourceFile(string sourceFile)
        {
            return iTunesTracks.Where(t => t.Comment.Contains(sourceFile)).First().Location;
        }
    }
}
