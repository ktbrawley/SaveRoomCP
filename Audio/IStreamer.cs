using System.Collections.Generic;
using System.Threading.Tasks;

namespace SaveRoomCP.Audio
{
    public interface IStreamer
    {
        Task<bool> SaveVideosAsWav(string outputDir, List<string> playlistVideoIds);
    }
}