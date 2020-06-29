using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SaveRoomCP.SoundSystem
{
    public class LinuxMusicPlayer : IPlayer
    {
        private readonly string MusicBasePath = $"{Environment.CurrentDirectory}/SaveRoomMusic";

        private Process _process;

        public Task LoadSong(string song)
        {
            return Task.CompletedTask;
        }

        public Task Play(string fileName)
        {
            _process = (StartAplayPlayback(fileName));
            return Task.CompletedTask;
        }

        public Task Stop()
        {
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
    }
}