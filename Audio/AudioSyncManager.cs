using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SaveRoomCP.Audio
{
    public class AudioSyncManager : ISyncManager
    {
        private readonly string OUTPUT = $"{new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName}/SaveRoomMusic";
        public async Task<bool> CheckNewSongsFromYouTubePlaylist(string playlistUrl)
        {
            var result = false;
            using (var streamer = new YoutubeStreamer())
            {
                result = await streamer.SaveVideosAsWav(OUTPUT, playlistUrl);
                return result;
            }
        }

        public async Task DownloadNewSongsAsync()
        {
            using (var streamer = new YoutubeStreamer())
            {
                await streamer.SaveVideoAsWav(OUTPUT, "https://www.youtube.com/playlist?list=PLSL0-UtF7g_qN9j-1WZi2fSD3Tfprjifi");
            }

        }
    }
}
