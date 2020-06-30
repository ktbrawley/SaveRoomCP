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

        [DllImport("winmm.dll")]
        private static extern long mciSendString(string command, StringBuilder stringReturn, int retunLength, IntPtr hwndCallback);
        public WindowsPlayer(string MUSIC_PATH)
        {   
           this.MUSIC_PATH = MUSIC_PATH;
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
            return Task.CompletedTask;
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