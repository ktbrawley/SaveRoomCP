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

        public async Task<bool> SaveVideosAsWav(string outputDir, List<string> playlistVideoIds)
        {
            var videoUrl = $"https://www.youtube.com/watch?v=id";
            var youtube = YouTube.Default;
            foreach (var id in playlistVideoIds)
            {
                var vid = await youtube.GetVideoAsync(videoUrl.Replace("id", id));
                var videoFileName = $"{outputDir}/{vid.FullName}";
                var newVidName = $"{outputDir}/{FormatFileName(vid.FullName)}";
                if (!File.Exists(newVidName))
                {
                    Console.WriteLine($"Downloading {vid.FullName}...");
                    File.WriteAllBytes($"{newVidName}", await vid.GetBytesAsync());
                }
            }
            AudioConvertor.ConvertBatchToWav(outputDir);

            return true;
        }

        private string FormatFileName(string fileName)
        {
            return fileName
                .Replace(" ", "_")
                .Replace(":", "")
                .Replace("\"", "")
                .Replace("(", "")
                .Replace("`", "")
                .Replace("'", "")
                .Replace(")", "");
        }
    }
}
