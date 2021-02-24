using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityNetwork;

public class ChatClient : MonoBehaviour
{
    public enum MessageID
    {
        Chat = 100,//本示例中唯一的聊天消息标识符（为什么这个标识符单独设置？）
    }

    ChatHandler ch;//处理网络事件

    //UI控件
    public Text revText;
    public InputField inputField;
    public Button sendBtn;

    private void Start()
    {
        ch = new ChatHandler();//创建实例

        //添加网络事件
        ch.AddHandler((short)TCPPeer.MessageID.OnConnected, OnConnected);
        ch.AddHandler((short)TCPPeer.MessageID.OnConnectFail, OnConnectedFail);
        ch.AddHandler((short)TCPPeer.MessageID.OnDisconnect, OnDisconnected);
        ch.AddHandler((short)MessageID.Chat, OnChat);

        //连接服务器
        ch.ConnectToServer();
        //UI事件
        sendBtn.onClick.AddListener(delegate ()
        {
            SendChat();//单击按钮发送消息
        });
    }

    private void Update()
    {
        ch.ProcessPackets();//处理服务器发来是数据包
    }

    //回调函数
    public void OnConnected(Packet packet)
    {
        Debug.Log("Link Server Successful");
    }
    public void OnConnectedFail(Packet packet)
    {
        Debug.Log("Link Server Fail, Please Quit");
    }
    public void OnDisconnected(Packet packet)
    {
        Debug.Log("Lost Link");
    }

    //UI事件回调函数，发送消息
    void SendChat()
    {
        //聊天协议
        ChatProto proto = new ChatProto();
        proto.userName = "client";
        proto.chatMsg = inputField.text;
        //序列化
        byte[] bs = Packet.Serialize<ChatProto>(proto);

        //创建数据包
        Packet p = new Packet((short)MessageID.Chat);
        using(MemoryStream stream = p.Stream)
        {
            BinaryWriter bw = new BinaryWriter(stream);
            bw.Write(bs.Length);
            bw.Write(bs);
            p.EncodeHeader(stream);
        }

        //发送给服务器
        ch.SendMessage(p);
        inputField.text = "";//清空输入框
    }

    //收到服务器发送的聊天消息
    public void OnChat(Packet packet)
    {
        byte[] buf = null;

        using(MemoryStream stream = packet.Stream)
        {
            BinaryReader br = new BinaryReader(stream);
            int len = br.ReadInt32();//读入长度
            buf = br.ReadBytes(len);//读入byte数组
        }

        ChatProto proto = Packet.Deserialize<ChatProto>(buf);
        revText.text = proto.userName + " : " + proto.chatMsg;
    }
}
