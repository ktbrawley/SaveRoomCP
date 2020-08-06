using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SaveRoomCP.Audio
{
    public static class AudioConvertor
    {
        public static void ConvertToWav(string sourceFilePath, string outputDirPath)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/usr/bin/ffmpeg",
                    Arguments = $"-i {sourceFilePath}.mp4 {sourceFilePath}.wav",
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();

        }

        public static void ConvertBatchToWav(string outputDirPath)
        {
            var cmd = $"cd {outputDirPath} && for file in *.mp4; do ffmpeg -i ${{file}} ${{file/%mp4/wav}}; done";
            var espacedArgs = cmd.Replace("\"", "\\\"");

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{espacedArgs}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
            RemoveVideoFiles(outputDirPath);
        }

        public static void RemoveVideoFiles(string outputDirPath)
        {
            var files = Directory.GetFiles(outputDirPath, "*.*").Where(f => f.EndsWith(".mp4"));
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }
    }
}

