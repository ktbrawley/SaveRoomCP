using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

namespace SaveRoomCP.SoundSystem
{
    public class LinuxMusicPlayer : IPlayer
    {
        private readonly string MusicBasePath = $"{Environment.CurrentDirectory}/SaveRoomMusic";
        private const int TIMEOUT_DELTA = 50;

        private Process _process;

        public Task Play(string fileName)
        {
            _process = (StartAplayPlayback(fileName));
            for (int i = 0; i < 100; i++)
            {
                AdjustVolume(true);
                Thread.Sleep(TIMEOUT_DELTA);
            }
            
            return Task.CompletedTask;
        }

        public Task Stop()
        {
            Console.WriteLine();
            Console.WriteLine("Stopping Music...");
            Console.WriteLine();

            for (int i = 100; i > 0; i--)
            {
                AdjustVolume(false);
                Thread.Sleep(TIMEOUT_DELTA);
            }
            if (_process != null)
            {
                _process.Kill();
                _process.Dispose();
                _process = null;
            }

            return Task.CompletedTask;
        }

        private Process StartAplayPlayback(string fileName)
        {
            var escapedArgs = fileName.Replace("\"", "\\\"");
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"aplay {escapedArgs}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            return process;
        }

        private void AdjustVolume(bool increaseVol)
        {
            string volumeDelta = increaseVol ? "1%+" : "1%-";
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"amixer -q sset Master {volumeDelta}",
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
        }
    }
}