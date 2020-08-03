using System;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace SaveRoomCP.SoundSystem
{
    public class WindowsPlayer : IPlayer
    {
        public Process CurrentProcess => throw new NotImplementedException();

        [DllImport("winmm.dll")]
        private static extern long mciSendString(string command, StringBuilder stringReturn, int retunLength, IntPtr hwndCallback);

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

            if (result != 0)
            {
                throw new Exception($"Error executing MSI command. Error code: {result}");
            }
        }
    }
}