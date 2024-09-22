using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SaveRoomCP.Audio;
using YT2AudioConverter;
using Logger = NLog.Logger;

namespace SaveRoomCP.SoundSystem
{
    public class SoundManager
    {
        private readonly string MUSIC_BASE_PATH = $"{new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName}/Files";
        private IPlayer _player;
        private List<string> _saveRoomSongs = new List<string>();
        private List<string> _playedSongs = new List<string>();
        private readonly IConfiguration _configuration;
        private static Logger _logger;

        public SoundManager(IConfiguration configuration, Logger logger)
        {
            _player = new MusicPlayer(logger);
            _configuration = configuration;
            _logger = logger;
        }

        public async Task CheckForNewSongs(string playlistId)
        {
            if (!Directory.Exists(MUSIC_BASE_PATH))
            {
                Directory.CreateDirectory(MUSIC_BASE_PATH);
            }

            var existingVideoCount = Directory.EnumerateFiles(MUSIC_BASE_PATH, "*.*", SearchOption.AllDirectories)
                .Where(f => f.EndsWith(".wav")).ToList().Count;

            if (existingVideoCount <= 0)
            {
                using (var util = new YoutubeUtils(_configuration))
                {
                    await util.ConvertYoutubeUriToFile(new YT2AudioConverter.Models.YoutubeToFileRequest
                    {
                        TargetMediaType = "wav",
                        Uri = _configuration.GetValue<string>("PlaylistUri")
                    });
                }
            }
        }

        public void SearchForSongs(out bool quitProgram)
        {
            _saveRoomSongs = Directory.GetFiles(MUSIC_BASE_PATH).ToList();

            if (_saveRoomSongs.Count <= 0)
            {
                _logger.Warn("No songs to play");
                quitProgram = true;
            }
            else
            {
                _logger.Info($"{_saveRoomSongs.Count} Save room songs found");
                Console.WriteLine();
            }
            quitProgram = false;
        }

        private Tuple<string, int> SelectRandomSong(bool randomize = true)
        {
            if (_saveRoomSongs.Count == 0)
            {
                ReloadSongs();
            }
            var increment = _playedSongs.Count() == 0 ? 0 : _playedSongs.Count();

            var nextSongIndex = randomize ? new Random((int)DateTime.Now.ToFileTime()).Next(0, (_saveRoomSongs.Count - 1)) : increment;
            return new Tuple<string, int>(_saveRoomSongs[nextSongIndex], nextSongIndex);
        }

        public string LoadSong(bool randomize = true)
        {
            var tuple = SelectRandomSong(randomize);
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
                .Where(f => f.EndsWith(".wav"))
                .ToList();

            return _saveRoomSongs.Count > 0;
        }

        private static string formatFileNumberForSort(string inVal)
        {
            int o;
            if (int.TryParse(Path.GetFileName(inVal), out o))
            {
                Console.WriteLine(string.Format("{0:0000000000}", o));
                return string.Format("{0:0000000000}", o);
            }
            else
                return inVal;
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

        public bool IsPlaying()
        {
            return _player.IsPlaying;
        }
    }
}