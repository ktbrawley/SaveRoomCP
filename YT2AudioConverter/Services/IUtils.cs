using System.Collections.Generic;
using System.Threading.Tasks;
using YT2AudioConverter.Models;

namespace YT2AudioConverter.Services
{
    public interface IUtils
    {
        Task<ConvertResponse> ConvertYoutubeUriToFile(YoutubeToFileRequest request);
    }
}