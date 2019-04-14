using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Force.Crc32;

namespace FLAC2iTunes
{
    public static class Helpers
    {
        public static string GetFileMD5(string path)
        {
            var file = new FileStream(path, FileMode.Open);
            var length = (int)file.Length;
            var data = new byte[length];

            file.Read(data, 0, length);
            file.Close();

            return Helpers.MD5(data);
        }

        public static string MD5(byte[] data)
        {
            // This is one implementation of the abstract class MD5.
            MD5 md5 = new MD5CryptoServiceProvider();

            byte[] result = md5.ComputeHash(data);

            return ToHex(result);
        }

        public static string ToHex(byte[] bytes, bool upperCase = false)
        {
            StringBuilder result = new StringBuilder(bytes.Length * 2);

            for (int i = 0; i < bytes.Length; i++)
                result.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));

            return result.ToString();
        }

        public static string GetFileCRC32(string path)
        {
            // Read by 512 bytes
            // No idea if this is accurate for detecting changes in files
            var file = new FileStream(path, FileMode.Open);
            var data = new byte[512];

            file.Read(data, 0, 512);
            file.Close();

            return Crc32CAlgorithm.Compute(data).ToString();
        }

        public static string GetFileSize(string path)
        {
            return new FileInfo(path).Length.ToString();
        }
    }
}
