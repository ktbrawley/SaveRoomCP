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
        private static string _youtubeApiKey = String.Empty;
        private static SerialPortManager _serialPortManager = new SerialPortManager();
        private static SoundManager _soundManager = new SoundManager();
        private static AudioSyncManager _audioSyncManager = new AudioSyncManager();

        private static readonly string playlistId = $"PLSL0-UtF7g_qN9j-1WZi2fSD3Tfprjifi";

        private static readonly List<string> _videoIds = new List<string> { };

        private static readonly string MUSIC_BASE_PATH = $"{new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName}/SaveRoomMusic";

        private static async Task Main(string[] args)
        {
            try
            {
                var serialPort = _serialPortManager.EstablishSerialPortCommunication(out quitProgram) ?? throw new Exception("No serial port available");

                if (quitProgram)
                {
                    return;
                }

                await CheckForNewSongs();

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

        private static void LoadConfiguration()
        {
            IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("./App_Config/appsettings.json", true, true)
            .Build();
            _youtubeApiKey = configuration.GetSection("YoutubeApiKey").Value;
        }

        private static async Task ExtractYoutubeVideoInfoFromPlaylist(string playlistId)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = _youtubeApiKey,
                ApplicationName = "SaveRoomCP"
            });

            var playlistRequest = youtubeService.PlaylistItems.List("snippet");
            playlistRequest.PlaylistId = playlistId;
            playlistRequest.MaxResults = 20;

            // Retrieve the list of videos uploaded to the authenticated user's channel.
            var playlistItemsListResponse = await playlistRequest.ExecuteAsync();

            foreach (var playlistItem in playlistItemsListResponse.Items)
            {
                // Print information about each video.
                // Console.WriteLine("{0} ({1})", playlistItem.Snippet.Title, playlistItem.Snippet.ResourceId.VideoId);
                _videoIds.Add(playlistItem.Snippet.ResourceId.VideoId);
            }
        }

        private static async Task CheckForNewSongs()
        {
            LoadConfiguration();

            await ExtractYoutubeVideoInfoFromPlaylist(playlistId);

            if (!Directory.Exists(MUSIC_BASE_PATH))
            {
                Directory.CreateDirectory(MUSIC_BASE_PATH);
            }

            var existingVideoCount = Directory.EnumerateFiles(MUSIC_BASE_PATH, "*.*", SearchOption.AllDirectories)
                .Where(f => f.EndsWith(".wav")).ToList().Count;

            if (_videoIds.Count > 0 && _videoIds.Count > existingVideoCount)
                await ((ISyncManager)_audioSyncManager).DownloadNewSongsAsync(_videoIds);
        }
    }
}