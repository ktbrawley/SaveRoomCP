using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;

namespace SaveRoomCP.SoundSystem
{
    public interface IPlayer
    {
        Task Play(string fileName);

        Task Stop();

        bool IsPlaying
        {
            get;
        }
    }
}