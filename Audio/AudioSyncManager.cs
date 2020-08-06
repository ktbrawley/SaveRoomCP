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
        public async Task DownloadNewSongsAsync(List<string> playlistVideoIds)
        {
            using (var streamer = new YoutubeStreamer())
            {
                await streamer.SaveVideosAsWav(OUTPUT, playlistVideoIds);
            }
        }
    }
}
