using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Xml;
using Blockgame_OpenTK.Util;
using LiteNetLib;
using LiteNetLib.Utils;
using NVorbis.Contracts;
using Tomlet;
using Tomlet.Attributes;

namespace Blockgame_OpenTK.Core.Networking;

public struct ServerProperties
{

    [TomlProperty("address")]
    public IPAddress Address { get; set; }
    
    [TomlProperty("port")]
    public int Port { get; set; }

    [TomlProperty("max_players")]
    public int MaxPlayers { get; set; }

    public ServerProperties(ServerProperties properties) 
    {

        // Address = IPAddress.Parse(AddressString);
    }

}
public class Server
{

    private RSACryptoServiceProvider _rsa;
    public byte[] PublicKey => _rsa.ExportRSAPublicKey();
    public bool IsMultiplayer;
    public EventBasedNetListener ServerListener;
    public NetManager ServerNetworkManager;
    public ServerProperties Properties { get; private set; }
    public Server(bool isMultiplayer = false)
    {

        IsMultiplayer = isMultiplayer;
        _rsa = new RSACryptoServiceProvider(2048);

    }

    public void Start()
    {

        if (IsMultiplayer)
        {

            ServerListener = new EventBasedNetListener();
            ServerNetworkManager = new NetManager(ServerListener);

            Properties = TomletMain.To<ServerProperties>(File.ReadAllText("server.toml"));
            
            ServerNetworkManager.IPv6Enabled = false;
            ServerNetworkManager.Start(Properties.Address.MapToIPv4(), null, Properties.Port);

            GameLogger.Log($"Started a server at {Properties.Address}:{Properties.Port}");

            ServerListener.ConnectionRequestEvent += request => 
            {

                if (ServerNetworkManager.ConnectedPeersCount >= Properties.MaxPlayers)
                {
                    NetDataWriter writer = new NetDataWriter();
                    writer.Put((byte)PacketType.ServerDisconnectErrorMaxPlayersPacket);
                    request.Reject(writer);
                } else
                {
                    request.AcceptIfKey("ClientGameConnection");
                }

            };

        } else
        {



        }

    }   

    public void Update()
    {

        if (IsMultiplayer)
        {



        } else
        {



        }

    }

    public void Stop()
    {

        if (IsMultiplayer)
        {

            ServerNetworkManager.Stop();

        } else
        {



        }

    }

}