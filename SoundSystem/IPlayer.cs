using System.Threading.Tasks;
using System.Collections.Generic;

namespace SaveRoomCP.SoundSystem
{
  public interface IPlayer   
  {
    Task LoadSong(string song);
    Task Play(string fileName);
    Task Stop();
  }
}