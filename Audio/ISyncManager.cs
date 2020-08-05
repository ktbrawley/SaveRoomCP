using System.Threading.Tasks;

namespace SaveRoomCP.Audio
{
    public interface ISyncManager
    {
        Task<bool> CheckNewSongsFromYouTubePlaylist(string playlistUrl);
        Task DownloadNewSongsAsync();
    }
}