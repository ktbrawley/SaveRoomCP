using System;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using NAudio.Wave;
using System.Threading;
using NAudio.Wave.SampleProviders;

namespace SaveRoomCP.SoundSystem
{
    public class WindowsPlayer : IPlayer
    {
        public bool IsPlaying { get; set; }

        public Process CurrentProcess => throw new NotImplementedException();

        private readonly IWavePlayer outputDevice;
        private readonly MixingSampleProvider mixer;

        public WindowsPlayer()
        {
            outputDevice = new WaveOutEvent();
            mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate: 48000, channels: 2));
            mixer.ReadFully = true;
            outputDevice.Init(mixer);
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

            var input = new AudioFileReader(escapedArgs);

            AddMixerInput(new AutoDisposeFileReader(input));

            IsPlaying = true;

            return Task.CompletedTask;
        }

        private void AddMixerInput(ISampleProvider input)
        {
            outputDevice.Play();
            mixer.AddMixerInput(ConvertToRightChannelCount(input));
        }

        private ISampleProvider ConvertToRightChannelCount(ISampleProvider input)
        {
            if (input.WaveFormat.Channels == mixer.WaveFormat.Channels)
            {
                return input;
            }
            if (input.WaveFormat.Channels == 1 && mixer.WaveFormat.Channels == 2)
            {
                return new MonoToStereoSampleProvider(input);
            }
            throw new NotImplementedException("Not yet implemented this channel count conversion");
        }

        public void Dispose()
        {
            outputDevice.Stop();
            mixer.RemoveAllMixerInputs();
        }

        /// <summary>
        /// Ends music stream
        /// </summary>
        /// <param name="isFirstPass"></param>
        public Task Stop()
        {
            Dispose();
            Console.WriteLine();
            Console.WriteLine("Stopping Music...");
            Console.WriteLine();
            return Task.CompletedTask;
        }
    }
}