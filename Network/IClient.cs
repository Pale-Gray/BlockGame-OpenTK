using Blockgame_OpenTK.Core.PlayerUtil;
using Blockgame_OpenTK.Core.Worlds;
using LiteNetLib.Utils;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK.Core.Networking;

public interface IClient 
{
    public abstract void Start(string addressOrHost = "", int port = 0, NewPlayer player = null);
    public abstract void Stop();
    public abstract void Update();
    public abstract void TickUpdate();
    public abstract void SendChunk(Vector2i position);

}