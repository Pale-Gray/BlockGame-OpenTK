using LiteNetLib;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK.Core.Networking;

public interface IServer 
{
    public abstract void Start();
    public abstract void Stop();
    public abstract void Update();
    public abstract void TickUpdate();
    public abstract void SendChunk(Vector2i position);

}