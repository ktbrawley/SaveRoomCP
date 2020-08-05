using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using SaveRoomCP.Audio;

namespace SaveRoomCP.SoundSystem
{
    public class SoundManager
    {
        private readonly string MUSIC_BASE_PATH = $"{new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName}/SaveRoomMusic";
        private IPlayer _player;
    
        private List<string> _saveRoomSongs = new List<string>();
        private List<string> _playedSongs = new List<string>();


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
        }

        public Process CurrentProcess()
        {
            return _player.CurrentProcess;
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
            _saveRoomSongs = Directory.GetFiles(MUSIC_BASE_PATH).ToList();
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