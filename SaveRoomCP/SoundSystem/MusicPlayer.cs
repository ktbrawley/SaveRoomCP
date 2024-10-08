﻿using System;
using System.Threading.Tasks;
using System.Diagnostics;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Threading;
using System.IO;
using System.Reflection;
using Logger = NLog.Logger;
using NAudio.Mixer;

namespace SaveRoomCP.SoundSystem
{
    public class MusicPlayer : IPlayer
    {
        private readonly string MUSIC_BASE_PATH = $"{new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName}/Files/RE Save Room Music/";
        public bool IsPlaying => (int)_outputDevice.PlaybackState == 1;

        public Process CurrentProcess => throw new NotImplementedException();

        private readonly IWavePlayer _outputDevice;
        private readonly MixingSampleProvider _mixer;
        private FadeInOutSampleProvider _fader;
        private static Logger _logger;

        public MusicPlayer(Logger logger, int sampleRate = 44100, int channels = 2)
        {
            _outputDevice = new WaveOutEvent();
            _mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels));
            _mixer.ReadFully = true;
            _outputDevice.Init(_mixer);
            _logger = logger;
        }

        /// <summary>
        /// Starts music stream
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="isFirstPass"></param>
        public Task Play(string fileName)
        {
            var path = MUSIC_BASE_PATH
                .Replace("/", @"\")
                .Replace(@"\\", @"\");

            var escapedArgs = fileName
                .Replace("/", @"\")
                .Replace(@"\\", @"\");

            InitAudioPlayback(new AudioFileReader(escapedArgs));

            Console.WriteLine();
            _logger.Info($"Now Playing: {escapedArgs.Replace(path, "")}");
            Console.WriteLine();

            return Task.CompletedTask;
        }

        private void AddMixerInput(ISampleProvider input)
        {
            _mixer.AddMixerInput(ConvertToRightChannelCount(input));
        }

        private ISampleProvider ConvertToRightChannelCount(ISampleProvider input)
        {
            input = new WdlResamplingSampleProvider(input, _mixer.WaveFormat.SampleRate);

            if (input.WaveFormat.Channels == _mixer.WaveFormat.Channels)
            {
                return input;
            }
            if (input.WaveFormat.Channels == 1 && _mixer.WaveFormat.Channels == 2)
            {
                return new MonoToStereoSampleProvider(input);
            }
            throw new NotImplementedException("Not yet implemented this channel count conversion");
        }

        private void InitAudioPlayback(AudioFileReader newAudioSource)
        {
            _fader = new FadeInOutSampleProvider(newAudioSource, true);

            // Prepare target sound device and play audio
            _outputDevice.Init(_fader);
            AddMixerInput(new AutoDisposeFileReader(newAudioSource));
            _outputDevice.Play();
            _fader.BeginFadeIn(2000);
        }

        /// <summary>
        /// Ends music stream
        /// </summary>
        /// <param name="isFirstPass"></param>
        public Task Stop()
        {
            // Stop all playing / fade audio
            _fader.BeginFadeOut(2000);
            Thread.Sleep(2000);

            _outputDevice.Stop();

            Console.WriteLine();
            _logger.Info("Stopping Music...");

            Console.WriteLine();

            // Reset audio inputs in preparation for next track
            _mixer.RemoveAllMixerInputs();
            return Task.CompletedTask;
        }
    }
}