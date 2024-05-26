using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using YT2AudioConverter.Models;

namespace YT2AudioConverter.Services
{
    public static class FileConverter
    {
        public static void ConvertBatchToWav(string outputDirPath)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info($"Converting audio to path: {outputDirPath}");

            var files = Directory.GetFiles(outputDirPath, "*.*")
                .Where(f => f.EndsWith(".mp4"));

            foreach (var file in files)
            {
                var request = GenerateConversionRequest(file.Replace(".mp4", ""), "mp3");
                ExecuteConversionRequest(request);
            }
            RemoveVideoFiles(outputDirPath);
        }

        public static void ConvertBatchToMp3(string outputDirPath)
        {

            var files = Directory.GetFiles(outputDirPath, "*.*")
                .Where(f => f.EndsWith(".mp4"));

            foreach (var file in files)
            {
                var request = GenerateConversionRequest(file.Replace(".mp4", ""), "mp3");
                ExecuteConversionRequest(request);
            }
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

        private static ConvertRequest GenerateConversionRequest(string sourceFilePath, string convertFileType)
        {
            var conversionRequest = new ConvertRequest();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                conversionRequest = new ConvertRequest()
                {
                    Cmd = "C:\\ProgramData\\chocolatey\\bin\\ffmpeg",
                    Args = $"-i {sourceFilePath}.mp4 {sourceFilePath}.{convertFileType}"
                };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                conversionRequest = new ConvertRequest()
                {
                    Cmd = "usr/bin/ffmpeg",
                    Args = $"-i {sourceFilePath}.mp4 {sourceFilePath}.{convertFileType}"
                };
            }
            return conversionRequest;
        }

        private static void ExecuteConversionRequest(ConvertRequest request)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = request.Cmd,
                    Arguments = request.Args,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
        }
    }
}