using System;
using System.Diagnostics;
using Blockgame_OpenTK.Util;
using LiteNetLib;

namespace Blockgame_OpenTK.Core.Networking;

public class Client
{

    public bool IsMultiplayer { get; private set; }
    public EventBasedNetListener ClientListener;
    public NetManager ClientNetworkManager;

    public Client(bool isMultiplayer = false)
    {

        IsMultiplayer = isMultiplayer;

    }

    public void JoinWorld(string worldName)
    {

        IsMultiplayer = false;

    }

    public void JoinWorld(string address, int port)
    {

        IsMultiplayer = true;
        ClientListener = new EventBasedNetListener();
        ClientNetworkManager = new NetManager(ClientListener);
        ClientNetworkManager.Start();
        ClientNetworkManager.Connect(address, port, "BlockGame");

        ClientListener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) =>
        {

            Console.WriteLine((PacketType)dataReader.GetByte());
            dataReader.Recycle();

        };

    }

    public void Update()
    {

        if (IsMultiplayer)
        {

            ClientNetworkManager.PollEvents();

        } else
        {

            

        }

    }

    public void Stop()
    {

        if (IsMultiplayer)
        {

            ClientNetworkManager.Stop();

        } else
        {



        }   

    }

}