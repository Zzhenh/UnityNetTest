using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using UnityNetwork;

public class ChatHandler : MyEventHandler
{
    TCPPeer peer = null;
    Socket socket = null;

    //连接服务器
    public void ConnectToServer()
    {
        peer = new TCPPeer(this);
        socket = peer.Connect("127.0.0.1", 8000);
    }

    //发送数据包
    public void SendMessage(Packet packet)
    {
        TCPPeer.Send(socket, packet);
    }
}
