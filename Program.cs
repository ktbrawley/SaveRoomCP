using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Google.Apis.Discovery;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Configuration;
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
        private static readonly string playlistId = ConfigurationManager.GetConfigurationValue("PlaylistId");

        private static async Task Main(string[] args)
        {
            try
            {
                var serialPort = _serialPortManager.EstablishSerialPortCommunication(out quitProgram) ?? throw new Exception("No serial port available");

                if (quitProgram)
                {
                    return;
                }

                await _soundManager.CheckForNewSongs(playlistId);

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
                    else if (!isLightOn && !isFirstPass)
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