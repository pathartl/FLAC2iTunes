using FLAC2iTunes.Models.Data.iTunes;
using FLAC2iTunes.Services;
using iTunesLib;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace FLAC2iTunes
{
    class Program
    {
        static void Main(string[] args)
        {
            var ConversionService = new ConversionService();

            ConversionService.Init();
            //ConversionService.ProcessUnsupportedMusic();
            ConversionService.ProcessSupportedMusic();

            Console.WriteLine("Done!");
        }
    }
}
