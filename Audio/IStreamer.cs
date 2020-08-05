using System.Threading.Tasks;

namespace SaveRoomCP.Audio
{
    public interface IStreamer
    {
        Task SaveVideoAsWav(string outputDir, string videoUrl, string newFileName);
        Task<bool> SaveVideosAsWav(string outputDir, string playlistUrl);
    }
}