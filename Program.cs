﻿using System;
using SaveRoomCP.SoundSystem;

namespace SaveRoomCP
{
    internal class Program
    {
        private static bool quitProgram = false;
        private static bool isFirstPass = true;
        private static SerialPortManager _serialPortManager = new SerialPortManager();
        private static SoundManager _soundManager;

        private static void Main(string[] args)
        {
            try
            {
                var serialPort = _serialPortManager.EstablishSerialPortCommunication(out quitProgram) ?? throw new Exception("No serial port available");

                if (quitProgram)
                {
                    return;
                }

                _soundManager = new SoundManager();
                _soundManager.SearchForSongs(out quitProgram);

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
                    else if (!isFirstPass && !isLightOn)
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