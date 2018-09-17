using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FLAC2iTunes.Models
{
    public class FFmpegCommand
    {
        private string FFmpegPath { get; set; }
        private Process Process { get; set; }

        public string InputPath { get; set; }
        public string OutputPath { get; set; }
        public string Codec { get; set; }
        public string Hash { get; set; }

        public FFmpegCommand()
        {
            FFmpegPath = ConfigurationManager.AppSettings["FFmpegPath"];
            Process = new Process();
            Process.StartInfo.FileName = FFmpegPath;
        }

        public void Convert()
        {
            Process.StartInfo.Arguments = String.Format(
                "-y -i \"{0}\" -acodec {1} -metadata comment=\"{2}\" -vcodec copy -sample_fmt s16p -ar 44100 -ac 2 \"{3}\"",
                InputPath.Replace("\\", "/"),
                Codec,
                Hash,
                OutputPath.Replace("\\", "/")
            );
            //Process.StartInfo.RedirectStandardOutput = true;
            //Process.StartInfo.RedirectStandardError = true;

            // hookup the eventhandlers to capture the data that is received
            //Process.OutputDataReceived += (sender, args) => sb.AppendLine(args.Data);
            //Process.ErrorDataReceived += (sender, args) => sb.AppendLine(args.Data);

            // direct start
            Process.StartInfo.UseShellExecute = false;
            Process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            Process.Start();
            // start our event pumps
            //Process.BeginOutputReadLine();
            //Process.BeginErrorReadLine();

            // until we are done
            Process.WaitForExit();
        }

        public void Convert(string input, string output, string codec)
        {
            InputPath = input;
            OutputPath = output;
            Codec = codec;
            Convert();
        }
    }
}
