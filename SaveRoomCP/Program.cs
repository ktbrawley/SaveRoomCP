using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SaveRoomCP.SoundSystem;
using Logger = NLog.Logger;

namespace SaveRoomCP
{
    internal class Program
    {
        private static bool quitProgram = false;
        private static bool isFirstPass = true;
        private static readonly string playlistId = ConfigurationManager.GetConfigurationValue("PlaylistId");
        private static Logger _logger;

        private static async Task Main(string[] args)
        {
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("App_Config/appsettings.json");
            IConfiguration configuration = configurationBuilder.Build();
            _logger = NLog.LogManager.GetCurrentClassLogger();

            var _serialPortManager = new SerialPortManager(configuration, _logger);
            var _soundManager = new SoundManager(configuration, _logger);

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
                                var song = _soundManager.LoadSong(randomize: false);
                                _soundManager.PlayMusic(song, out isFirstPass);
                                break;

                            case false:
                                isFirstPass = !_soundManager.IsPlaying();
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
                _logger.Error($"{ex.Message}");
            }
            finally
            {
                Console.ReadKey();
            }
        }
    }
}