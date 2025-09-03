using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using LiteNetLib;

namespace VoxelGame.Networking;

public abstract class Networked
{
    public EventBasedNetListener Listener = new EventBasedNetListener();
    public NetManager Manager;
    public DataWriter Writer = new DataWriter();
    
    public Dictionary<NetPeer, Player> ConnectedPlayers = new();

    public string HostOrIp;
    public int Port;

    public Networked()
    {
        Manager = new NetManager(Listener);
    }

    public Networked(string hostOrIp, int port)
    {
        HostOrIp = hostOrIp;
        Port = port;

        Manager = new NetManager(Listener);
    }

    public Networked(string pathToConfig)
    {
        using (FileStream stream = File.OpenRead(pathToConfig))
        {
            Dictionary<string, object> serverSettings = JsonSerializer.Deserialize<Dictionary<string, object>>(stream)!;
            HostOrIp = serverSettings["ip"].ToString()!;
            Port = int.Parse(serverSettings["port"].ToString()!);
        }

        Manager = new NetManager(Listener);
    }

    public abstract void Start();
    public abstract void Stop();
    public abstract void Update();
    public abstract void TickUpdate();
    public abstract void Join(bool isInternal = false);
    public abstract void Disconnect();
    public void SendPacket(IPacket packet, NetPeer? excludingPeer = null)
    {
        Writer.Clear();
        Writer.Write((int)packet.Type);
        packet.Serialize(Writer);
        if (excludingPeer == null)
        {
            Manager.SendToAll(Writer.Data, DeliveryMethod.ReliableOrdered);
        }
        else
        {
            Manager.SendToAll(Writer.Data, DeliveryMethod.ReliableOrdered, excludingPeer);
        }
    }

    public void SendPacketTo(IPacket packet, NetPeer peer)
    {
        Writer.Clear();
        Writer.Write((int)packet.Type);
        packet.Serialize(Writer);
        peer.Send(Writer.Data, DeliveryMethod.ReliableOrdered);
    }
}