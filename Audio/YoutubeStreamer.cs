using System;
using System.IO;
using System.Threading.Tasks;
using VideoLibrary;
using SoxSharp;
using System.Collections.Generic;
using System.Linq;

namespace SaveRoomCP.Audio
{
    public class YoutubeStreamer : IStreamer, IDisposable
    {
        private List<YouTubeVideo> _youtubeVideos;

        public YoutubeStreamer()
        {
            _youtubeVideos = new List<YouTubeVideo>();
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);  
        }

        public async Task<bool> SaveVideosAsWav(string outputDir, string playlistUrl)
        {
            var result = await CheckForNewSongsAsync(outputDir, playlistUrl);

            if (result)
            {
                Console.WriteLine($"Downloading new songs from playlist");
                var fileNames = new List<string>();
                foreach (var vid in _youtubeVideos)
                {
                    await ConvertVideoToWavAsync(vid, outputDir);
                }
                AudioConvertor.ConvertBatchToWav(outputDir);

                return true;
            }
            Console.WriteLine($"No new songs to download.");
            return false;
        }

        public async Task SaveVideoAsWav(string outputDir, string videoUrl, string newFileName = "")
        {
            var youtube = YouTube.Default;
            var vid = await youtube.GetVideoAsync(videoUrl);
            var videoFileName = $"{outputDir}/{vid.FullName}";
            var newVidName = string.IsNullOrEmpty(newFileName) ? $"{outputDir}/{vid.FullName.Substring(0, 5)}" : newFileName;

            File.WriteAllBytes($"{newVidName}.mp4", await vid.GetBytesAsync());

            ConvertVideoToWav(newVidName);

            File.Delete($"{newVidName}.mp4");
        }

        public async Task<bool> CheckForNewSongsAsync(string outputDir, string playlistUrl)
        {
            int songCount = Directory.GetFiles(outputDir).Count();

            var youtube = YouTube.Default;
            var vids = await youtube.GetAllVideosAsync(playlistUrl);
            _youtubeVideos = vids.ToList();

            return (_youtubeVideos.Count() > songCount);
        }

        private void ConvertVideoToWav(string videoPath)
        {
            AudioConvertor.ConvertToWav(videoPath, "");
        }

        private async Task ConvertVideoToWavAsync(YouTubeVideo vid, string outputDir, string newFileName = "")
        {
            var videoFileName = $"{outputDir}/{vid.FullName}";
            var newVidName = string.IsNullOrEmpty(newFileName) ? $"{outputDir}/{vid.FullName.Substring(0, 5)}" : newFileName;

            File.WriteAllBytes($"{newVidName}.mp4", await vid.GetBytesAsync());
            Console.WriteLine($"Downloading: {vid.FullName}");
        }
    }
}
