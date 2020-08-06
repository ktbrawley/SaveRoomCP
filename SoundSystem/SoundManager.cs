using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Configuration;
using SaveRoomCP.Audio;

namespace SaveRoomCP.SoundSystem
{
    public class SoundManager
    {
        private readonly string MUSIC_BASE_PATH = $"{new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName}/SaveRoomMusic";
        private IPlayer _player;
        private List<string> _saveRoomSongs = new List<string>();
        private List<string> _playedSongs = new List<string>();

        private string _youtubeApiKey = String.Empty;
        private readonly List<string> _videoIds = new List<string> { };

        private readonly AudioSyncManager _audioSyncManager;


        public SoundManager()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _player = new WindowsPlayer();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                _player = new LinuxMusicPlayer();
            }

            _audioSyncManager = new AudioSyncManager();
            _youtubeApiKey = ConfigurationManager.GetConfigurationValue("YoutubeApiKey");

        }

        public Process CurrentProcess()
        {
            return _player.CurrentProcess;
        }

        public async Task CheckForNewSongs(string playlistId)
        {
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

        private async Task ExtractYoutubeVideoInfoFromPlaylist(string playlistId)
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

        public void SearchForSongs(out bool quitProgram)
        {
            _saveRoomSongs = Directory.GetFiles(MUSIC_BASE_PATH).ToList();

            if (_saveRoomSongs.Count <= 0)
            {
                Console.WriteLine("No songs to play");
                quitProgram = true;
            }
            else
            {
                Console.WriteLine($"{_saveRoomSongs.Count} Save room songs found");
                Console.WriteLine();
            }
            quitProgram = false;
        }

        private Tuple<string, int> SelectRandomSong()
        {
            if (_saveRoomSongs.Count == 0)
            {
                ReloadSongs();
            }
            var randomIndex = new Random((int)DateTime.Now.ToFileTime()).Next(0, (_saveRoomSongs.Count - 1));
            return new Tuple<string, int>(_saveRoomSongs[randomIndex], randomIndex);
        }

        public string LoadSong()
        {
            var tuple = SelectRandomSong();
            var song = tuple.Item1;
            var songIndex = tuple.Item2;

            MarkSongAsPlayed(song, songIndex);

            return song;
        }

        private void MarkSongAsPlayed(string song, int songIndex)
        {
            _playedSongs.Add(song);
            _saveRoomSongs.RemoveAt(songIndex);
        }


        private bool ReloadSongs()
        {
            _saveRoomSongs = Directory.EnumerateFiles(MUSIC_BASE_PATH, "*.*", SearchOption.AllDirectories)
                .Where(f => f.EndsWith(".wav")).ToList();

            return _saveRoomSongs.Count > 0;
        }

        public bool PlayMusic(string song, out bool isFirstPass)
        {
            isFirstPass = true;

            var task = _player.Play(song);
            var hasFinished = task.IsCompleted;

            if (hasFinished)
            {
                isFirstPass = false;
            }

            return hasFinished;
        }

        public void StopMusic(out bool isFirstPass)
        {
            _player.Stop();
            isFirstPass = true;
        }
    }
}