using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if WINSYS
using System.Media;
using AudioSwitcher.AudioApi.CoreAudio;
#endif
using System.Threading;
using SaveRoomCP.SoundSystem;

namespace SaveRoomCP.SoundSystem
{
    public class WindowsPlayer : IPlayer
    {
        private readonly string MUSIC_PATH;
        private readonly int STARTING_VOL;
        private readonly int TIMEOUT_DELTA = 125;

        #if WINSYS
        private SoundPlayer _player;
        private CoreAudioDevice _defaultPlaybackDevice;
        #endif
       

        public WindowsPlayer(string MUSIC_PATH)
        {   
            this.MUSIC_PATH = MUSIC_PATH;
            #if WINSYS
            _player = new SoundPlayer();
            _defaultPlaybackDevice = new CoreAudioController()._defaultPlaybackDevice;
            STARTING_VOL = (int)_defaultPlaybackDevice.Volume;
            #endif
        }

        /// <summary>
        /// Returns the file path to a randomly selected song
        /// </summary>
        /// <returns>string</returns>
        public Task LoadSong(string song)
        {
            var message = $"Playing {song.Replace($"{MUSIC_PATH}/SaveRoomMusic", "")}...";

            #if WINSYS
            _player.SoundLocation = song;
            _player.Load();
            #endif

            return Task.FromResult(message);
        }

        /// <summary>
        /// Starts music stream
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="isFirstPass"></param>
        public Task Play(string fileName)
        {
            #if WINSYS
            if (_player.IsLoadCompleted)
            {
                _defaultPlaybackDevice.Volume = 0;
                _player.Play();

                for (int i = 0; i < (STARTING_VOL + 1); i++)
                {
                    _defaultPlaybackDevice.Volume = i;
                    Thread.Sleep(timeoutDelta);
                }

                
                Console.WriteLine();
                Console.WriteLine(fileName);
                Console.WriteLine();
            }
            #endif

            return Task.CompletedTask;
        }

        /// <summary>
        /// Ends music stream
        /// </summary>
        /// <param name="isFirstPass"></param>
        public Task Stop()
        {
            #if WINSYS
            for (int i = (int)_defaultPlaybackDevice.Volume; i > -1; i--)
            {
                _defaultPlaybackDevice.Volume = i;
                Thread.Sleep(timeoutDelta);
            }
            _player.Stop();
            #endif

            Console.WriteLine();
            Console.WriteLine("Stopping Music...");
            Console.WriteLine();
            ResetVolume();
            return Task.CompletedTask;
        }

        public void ResetVolume()
        {
            #if WINSYS
            for (int i = 0; i < (STARTING_VOL + 1); i++)
            {
                _defaultPlaybackDevice.Volume = i;
            }
            #endif
        }
    }
}