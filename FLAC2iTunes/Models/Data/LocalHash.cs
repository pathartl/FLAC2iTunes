using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FLAC2iTunes.Models.Data
{
    public class TrackFingerprint
    {
        public string File { get; set; }
        public string Hash { get; set; }
        public string FileSize { get; set; }

        public TrackFingerprint(string file, string hash, string fileSize)
        {
            File = file;
            Hash = hash;
            FileSize = fileSize;
        }

        public TrackFingerprint(string serialized)
        {
            FromString(serialized);
        }

        public TrackFingerprint()
        {

        }

        public override string ToString()
        {
            return String.Format("{0}|{1}|{2}", File, Hash, FileSize);
        }

        public void FromString(string input)
        {
            var exploded = input.Split('|');
            File = exploded[0];
            Hash = exploded[1];
            FileSize = exploded[2];
        }
    }
}
