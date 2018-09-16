using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTunesLib;

namespace FLAC2iTunes.Models.Data.iTunes
{
    public class Track
    {
        public string Album { get; set; }
        public string AlbumArtist { get; set; }
        public int AlbumRating { get; set; }
        public int AlbumRatingKind { get; set; }
        public string Artist { get; set; }
        public int BitRate { get; set; }
        public int BookmarkTime { get; set; }
        public int BPM { get; set; }
        public string Category { get; set; }
        public string Comment { get; set; }
        public bool Compilation { get; set; }
        public DateTime DateAdded { get; set; }
        public string Description { get; set; }
        public int DiscCount { get; set; }
        public int DiscNumber { get; set; }
        public int Duration { get; set; }
        public bool Enabled { get; set; }
        public int EpisodeNumber { get; set; }
        public bool ExcludeFromShuffle { get; set; }
        public int Finish { get; set; }
        public string Genre { get; set; }
        public int Index { get; set; }
        public int Kind { get; set; }
        public string KindAsString { get; set; }
        public string Location { get; set; }
        public string LongDescription { get; set; }
        public string Lyrics { get; set; }
        public DateTime ModificationDate { get; set; }
        public string Name { get; set; }
        public bool PartOfGaplessAlbum { get; set; }
        public int PlayedCount { get; set; }
        public DateTime PlayedDate { get; set; }
        public int PlayOrderIndex { get; set; }
        public bool Podcast { get; set; }
        public int Rating { get; set; }
        public DateTime ReleaseDate { get; set; }
        public bool RememberBookmark { get; set; }
        public int SampleRate { get; set; }
        public int SeasonNumber { get; set; }
        public int Size { get; set; }
        public int Size64High { get; set; }
        public int Size64Low { get; set; }
        public int SkippedCount { get; set; }
        public DateTime SkippedDate { get; set; }
        public string SortAlbum { get; set; }
        public string SortAlbumArtist { get; set; }
        public string SortArtist { get; set; }
        public string SortComposer { get; set; }
        public string SortName { get; set; }
        public string SortShow { get; set; }
        public int Start { get; set; }
        public string Time { get; set; }
        public int TrackCount { get; set; }
        public int TrackNumber { get; set; }
        public bool Unplayed { get; set; }
        public int VolumeAdjustment { get; set; }
        public int Year { get; set; }

        public Track(dynamic track)
        {
            Album = track.Album;
            AlbumArtist = track.AlbumArtist;
            AlbumRating = track.AlbumRating;
            AlbumRatingKind = track.AlbumRatingKind;
            Artist = track.Artist;
            BitRate = track.BitRate;
            BookmarkTime = track.BookmarkTime;
            BPM = track.BPM;
            Category = track.Category;
            Comment = track.Comment;
            Compilation = track.Compilation;
            DateAdded = track.DateAdded;
            Description = track.Description;
            DiscCount = track.DiscCount;
            DiscNumber = track.DiscNumber;
            Duration = track.Duration;
            Enabled = track.Enabled;
            EpisodeNumber = track.EpisodeNumber;
            ExcludeFromShuffle = track.ExcludeFromShuffle;
            Finish = track.Finish;
            Genre = track.Genre;
            Index = track.Index;
            Kind = track.Kind;
            KindAsString = track.KindAsString;
            Location = track.Location;
            LongDescription = track.LongDescription;
            Lyrics = track.Lyrics;
            ModificationDate = track.ModificationDate;
            Name = track.Name;
            PartOfGaplessAlbum = track.PartOfGaplessAlbum;
            PlayedCount = track.PlayedCount;
            PlayedDate = track.PlayedDate;
            PlayOrderIndex = track.PlayOrderIndex;
            Podcast = track.Podcast;
            Rating = track.Rating;
            ReleaseDate = track.ReleaseDate;
            RememberBookmark = track.RememberBookmark;
            SampleRate = track.SampleRate;
            SeasonNumber = track.SeasonNumber;
            Size = track.Size;
            Size64High = track.Size64High;
            Size64Low = track.Size64Low;
            SkippedCount = track.SkippedCount;
            SkippedDate = track.SkippedDate;
            SortAlbum = track.SortAlbum;
            SortAlbumArtist = track.SortAlbumArtist;
            SortArtist = track.SortArtist;
            SortComposer = track.SortComposer;
            SortName = track.SortName;
            SortShow = track.SortShow;
            Start = track.Start;
            Time = track.Time;
            TrackCount = track.TrackCount;
            TrackNumber = track.TrackNumber;
            Unplayed = track.Unplayed;
            VolumeAdjustment = track.VolumeAdjustment;
            Year = track.Year;
        }
    }
}
