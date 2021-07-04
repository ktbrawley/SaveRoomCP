using System.Collections.Generic;
using System.Threading.Tasks;

namespace SaveRoomCP.Audio
{
    public interface ISyncManager
    {
        Task DownloadNewSongsAsync(List<string> playlistVideoIds);
    }
}