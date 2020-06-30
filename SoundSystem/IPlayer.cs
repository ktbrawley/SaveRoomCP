using System.Threading.Tasks;
using System.Collections.Generic;

namespace SaveRoomCP.SoundSystem
{
  public interface IPlayer   
  {
    Task Play(string fileName);
    Task Stop();
  }
}