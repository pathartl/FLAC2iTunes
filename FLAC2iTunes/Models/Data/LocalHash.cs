using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FLAC2iTunes.Models.Data
{
    public class LocalHash
    {
        public string File { get; set; }
        public string Hash { get; set; }

        public LocalHash(string file, string hash)
        {
            File = file;
            Hash = hash;
        }
    }
}
