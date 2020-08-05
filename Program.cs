using System;
using System.IO;
using System.Reflection;
using SaveRoomCP.Audio;
using SaveRoomCP.SoundSystem;

namespace SaveRoomCP
{
    internal class Program
    {
        private static bool quitProgram = false;
        private static bool isFirstPass = true;
        private static SerialPortManager _serialPortManager = new SerialPortManager();
        private static SoundManager _soundManager = new SoundManager();
        private static AudioSyncManager _audioSyncManager = new AudioSyncManager();

        private static readonly string playlistUrl = $"https://www.youtube.com/watch?v=RjHzc5NAVrc&list=PLSL0-UtF7g_qN9j-1WZi2fSD3Tfprjifi";

        private static async System.Threading.Tasks.Task Main(string[] args)
        {
            try
            {
                var serialPort = _serialPortManager.EstablishSerialPortCommunication(out quitProgram) ?? throw new Exception("No serial port available");

                await _audioSyncManager.CheckNewSongsFromYouTubePlaylist(playlistUrl);

                if (quitProgram)
                {
                    return;
                }

                while (!quitProgram)
                {
                    var isLightOn = _serialPortManager.IsTheLightOn(serialPort);

                    if (isLightOn)
                    {
                        switch (isFirstPass)
                        {
                            case true:
                                var song = _soundManager.LoadSong();
                                _soundManager.PlayMusic(song, out isFirstPass);
                                break;
                            case false:
                                isFirstPass = _soundManager.CurrentProcess().HasExited;
                                break;
                        }
                    }
                    else
                    {
                        _soundManager.StopMusic(out isFirstPass);
                    }
                }
                _soundManager.StopMusic(out isFirstPass);
                return;


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error establishing communication with port: {ex.Message}");
            }
        }
    }
}