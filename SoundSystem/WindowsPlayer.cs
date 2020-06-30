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
using System.Runtime.InteropServices;

namespace SaveRoomCP.SoundSystem
{
    public class WindowsPlayer : IPlayer
    {
        private readonly string MUSIC_PATH;
        private readonly int STARTING_VOL;
        private readonly int TIMEOUT_DELTA = 125;

        [DllImport("winmm.dll")]
        private static extern long mciSendString(string command, StringBuilder stringReturn, int retunLength, IntPtr hwndCallback);
        public WindowsPlayer(string MUSIC_PATH)
        {   
           this.MUSIC_PATH = MUSIC_PATH;
        }

        /// <summary>
        /// Returns the file path to a randomly selected song
        /// </summary>
        /// <returns>string</returns>
        public Task LoadSong(string song)
        {

            return Task.CompletedTask;
        }

        /// <summary>
        /// Starts music stream
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="isFirstPass"></param>
        public Task Play(string fileName)
        {
             var escapedArgs = 
             fileName
             .Replace("/", @"\")
             .Replace(@"\\", @"\");
            Console.WriteLine(escapedArgs);
            ExecuteMsiCommand("close all");
            ExecuteMsiCommand($"open {fileName} Alias player");
            ExecuteMsiCommand($"play player from 0");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Ends music stream
        /// </summary>
        /// <param name="isFirstPass"></param>
        public Task Stop()
        {
            ExecuteMsiCommand($"Stop player");
            Console.WriteLine();
            Console.WriteLine("Stopping Music...");
            Console.WriteLine();
            ResetVolume();
            return Task.CompletedTask;
        }

        public void ResetVolume()
        {
            
        }

        private void ExecuteMsiCommand(string commandString)
        {
            var result = mciSendString(commandString, null, 0, IntPtr.Zero);

            if(result != 0)
            {
                throw new Exception($"Error executing MSI command. Error code: {result}");
            }
        }
    }
}