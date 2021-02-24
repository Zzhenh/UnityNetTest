# TCP与UDP－网络开发

### 网络开发简介

1.客户端和服务器的传输举例
```c#
//传输一个整数，将整数转换为字节流
public byte[] WriteInt(int number)
{
	//将32位的int型整数转换为4个字节的byte数组
	byte[] bs = System.BitConvert.GetBytes(number);
	return bs;
}

public int ReadInt(byte[] bs)
{
	//将byte数组转换为32位整型数字
	int number = System.BitConvert.ToInt32(bs, 0);
	return number;
}
```
2.将对象序列化和反序列化
```c#
using System;
using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;

//定义一个类用于序列化，必须添加Serializable属性才可以序列化
[Serializable]
public class Player
{
	public int id;
	public string name;
	public int life;
}

public class Test : MonoBehaviour
{
	void Start()
	{
		//初始化
		Player player = new Player();
		player.id = 1;
		player.name = "zhangsan";
		player.life = 100;
		
		using(MemoryStream stream = new MemoryStream())
		{
			//创建序列化类
			BinaryFormatter bf = new BinaryFormatter();
			//将player序列化到stream中
			bf.Serialize(stream, player);
			stream.Seek(0, SeekOrigin.Begin);
			
			//将stream中的二进制数据反序列化到player2
			Player player2 = (Player)bf.Deserialize(stream);
			//打印palyer2
			Debug.log(String.Format("{0},{1},{2}", player2.id, player2.name, player2.life));
		}
	}
}
```

### 简单的网络通信程序

**简单的TCP程序**

1.新建c#控制台程序，服务器代码
```c#
using System;
using System.Net;
using System.Net.Sockets;

namespace UnityTCPSimpleServer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //指定8000端口作为服务器端口
                IPEndPoint ipe = new IPEndPoint(IPAddress.Any, 8000);
                //创建基于TCP流的Socket
                Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //将Socket绑定到服务器端口
                listener.Bind(ipe);
                //服务器开始监听，其中的backlog参数值指监听队列的长度
                listener.Listen(128);
                Console.WriteLine("Start Listen...");
                //开始接受客户端请求，程序在这里堵塞
                Socket mySocket = listener.Accept();
                Console.WriteLine("New Link From {0}", mySocket.RemoteEndPoint);

                //开始接受客户端的数据，程序会在这里堵塞
                byte[] bs = new byte[256];
                int length = mySocket.Receive(bs);

                //将客户端的数据返回给客户端
                mySocket.Send(bs, length, SocketFlags.None);

                //关闭与客户端的连接
                mySocket.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }


            Console.ReadLine();
        }
    }
}

```
创建一个Socket，绑定到一个端口上，然后开始监听客户端的连接，在接收客户端的连接后，将数据存储到byte数组中，并返回给客户端。
该程序是同步方式，程序会在等待连接时堵塞。
2.新建c#控制台程序，客户端代码
```c#
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace UnityTCPSimpleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //设置服务器的地址和端口
                IPEndPoint ipe = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8000);
                //创建TCPSocket
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //开始连接服务器，程序这里会堵塞
                client.Connect(ipe);
                Console.WriteLine("Link Server");
                //向服务器发送数据
                string data = "hello,world!";
                byte[] bs = UTF8Encoding.UTF8.GetBytes(data);
                client.Send(bs);
                //用一个数组保存服务器返回的数据
                byte[] rev = new byte[256];
                //接受服务器返回的数据，返回值是字节数组的长度
                int length = client.Receive(rev);
                Console.WriteLine(UTF8Encoding.UTF8.GetString(rev, 0, length));
                //关闭连接
                client.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }


            Console.ReadLine();
        }
    }
}

```
客户端代码与服务器类似，但是客户端需要主动连接服务器。
该程序也是同步方式，程序在连接服务器时也会堵塞直到收到服务器的响应。

**简单的UDP程序

1.创建UDP服务端工程
```c#
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
namespace UnityUDPSimpleServer
{
	class Program
	{
		static void Main(string[] args)
		{
			//创建UDPSocket
			Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			//将Socket绑定到8001端口
			server.Bind(new IPEndPoint(IPAddress.Any, 8001));
			
			byte[] buffer = new byte[256];
			EndPoint = new IPEndPoint(IPAddress.Any, 0);
			//开始接收数据，UDP不保证可靠性
			int length = server.RecieveFrom(buffer, SocketFlags.None, ref remoteIP);
			
			IPEndPoint remote = ((IPEndPoint)remoteIP);
			Console.WriteLine("Recieve data ({2}):{3} from {0}:{1}", remote.Address, remote.Port, length, UTF8Encoding.UTF8.GetString(buffer, 0, length));
			//返回数据给客户端
			server.SendTo(buffer, length, SocketFlags.None,remoteIP);
			
			server.Close();
			
			Console.ReadLine();
		}
	}
}
```
2.创建UDP客户端工程
```c#
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
namespace UnityUDPSimpleClient
{
	class Program
	{
		static void Main(string[] args)
		{
			Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			
			byte[] buffer = UTF8Encoding.UTF8.GetBytes("hello world");
			//UDP不需要连接服务器，直接向服务器的地址发送数据
			client.SendTo(buffer, new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8001));
			
			byte[] revbuffer = new byte[256];
			int length = client.Recieve(revbuffer);
			
			Console.WriteLine("Recieve:" + UTF8Encoding.UTF8.GetString(revbuffer, 0, length));
			
			
			Console.ReadLine();
		}
	}
}
```

### 异步TCP网络通信

1.创建数据包对象
在TCP传输网络数据的时候，接收方一次收到的数据长度可能是不确定的，比如客户端发送了100个字节给服务器，服务器可能一次收到100个字节，也可能先收到20个。
因此我们先创建一个类，用于管理序列化的数据流，用来序列化、反序列化对象。
创建一个c#的ClassLibrary工程，创建Packet.cs
```c#
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace UnityNetwork
{
    class Packet
    {
        //数据包头4个字节作为保留字节
        //0-1个字节用来保存数据长度
        //2-3个字节用来保存消息id
        public const int headerLength = 4;
        public short msgid = 0;//消息id占两个字节
        public Socket sk = null;//接收数据的Socket
        public byte[] buffer = new byte[1024];//用于保存数据的数组
        public int readLength = 0;//TCP数据流读取的字节长度
        public short bodyLength = 0;//有效数据的长度，占两个字节
        public bool encoded = false;//标志是否处理过四个字节

        public Packet(short id, Socket s = null)
        {
            msgid = id;
            sk = s;
            byte[] bs = BitConverter.GetBytes(id);
            bs.CopyTo(buffer, 2);
        }

        //复制构造函数
        public Packet(Packet p)
        {
            msgid = p.msgid;
            sk = p.sk;
            p.buffer.CopyTo(buffer, 0);
            bodyLength = p.bodyLength;
            readLength = p.readLength;
            encoded = p.encoded;
        }

        //重置
        public void ResetParams()
        {
            msgid = 0;
            bodyLength = 0;
            readLength = 0;
            encoded = false;
        }

        //将short类型的数据长度转为2个字节保存到byte数组的最前面
        public void EncodeHeader(MemoryStream stream)
        {
            if (stream != null)
                bodyLength = (short)stream.Position;

            byte[] bs = BitConverter.GetBytes(bodyLength);
            bs.CopyTo(buffer, 0);
            encoded = true;
        }

        //从byte数组头四个字节中解析出数据的长度和消息id
        public void DecodeHeader()
        {
            bodyLength = BitConverter.ToInt16(buffer, 0);
            msgid = BitConverter.ToInt16(buffer, 2);
        }

        /*MemoryStream位于System.IO命名空间，为系统内存提供流式的读写操作。
         * 常作为其他流数据交换时的中间对象操作。
         * 1、MemoryStream类封装一个字节数组，在构造实例时可以使用一个字节数组作为参数，
         * 但是数组的长度无法调整。使用默认无参数构造函数创建实例，
         * 可以使用Write方法写入，随着字节数据的写入，数组的大小自动调整。
         * 2、在对MemoryStream类中数据流进行读取时，可以使用seek方法定位读取器的当前的位置，
         * 可以通过指定长度的数组一次性读取指定长度的数据。
         * ReadByte方法每次读取一个字节，并将字节返回一个整数值。
         * 3、UnicodeEncoding类中定义了Unicode中UTF-16编码的相关功能。
         * 通过其中的方法将字符串转换为字节，也可以将字节转换为字符串。
         */

        //用于读写流数据
        public MemoryStream Stream
        {
            get
            {
                return new MemoryStream(buffer, headerLength, buffer.Length - headerLength);
            }
        }

        //序列化对象，这里使用的是c#自带的序列化类，也可替换为Json等序列化方式
        public static byte[] Serialize<T>(T t)
        {
            using(MemoryStream stream = new MemoryStream())
            {
                try
                {
                    //BinaryFormatter:以二进制格式序列化和反序列化对象

                    //创建序列化类
                    BinaryFormatter bf = new BinaryFormatter();
                    //序列化到stream中
                    bf.Serialize(stream, t);
                    stream.Seek(0, SeekOrigin.Begin);
                    return stream.ToArray();
                }
                catch(Exception e)
                {
                    return null;
                }
            }
        }

        //反序列化对象
        public static T Deserialize<T>(byte[] bs)
        {
            using(MemoryStream stream = new MemoryStream(bs))
            {
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    T t = (T)bf.Deserialize(stream);
                    return t;
                }
                catch(Exception e)
                {
                    return default(T);
                }
            }
        }
    }
}

```
2.逻辑处理
创建MyEventHandler.cs，这个类的作用是逻辑处理，一部分注册网络消息，是每个消息标识符与一个回调函数相关联，另一部分是将数据包入队并分发给响应的回调函数
```c#
using System;
using System.Collections.Generic;
using System.Text;

namespace UnityNetwork
{
    class MyEventHandler
    {
        public delegate void OnReceive(Packet packet);//回调函数
        protected Dictionary<int, OnReceive> handlers;//每个事件对应一个OnReceive函数
        protected Queue<Packet> Packets = new Queue<Packet>();//存储数据包的队列

        public MyEventHandler()
        {
            handlers = new Dictionary<int, OnReceive>();
        }

        //添加网络事件
        public virtual void AddHandler(int msgid, OnReceive handler)
        {
            handlers.Add(msgid, handler);
        }

        //将数据包入队，然后在ProcessPackets函数中处理数据包
        //网络和逻辑处理可能是在不同的线程中
        //所以入队出队的时候使用了lock防止多线程带来的问题
        public virtual void AddPacket(Packet packet)
        {
            /*lock 关键字可以用来确保代码块完成运行，而不会被其他线程中断。
             * 它可以把一段代码定义为互斥段（critical section），
             * 互斥段在一个时刻内只允许一个线程进入执行，而其他线程必须等待。
             * 这是通过在代码块运行期间为给定对象获取互斥锁来实现的。
             */

            lock(Packets)
            {
                Packets.Enqueue(packet);
            }
        }

        //数据包出队
        public Packet GetPacket()
        {
            lock(Packets)
            {
                if(Packets.Count == 0)
                {
                    return null;
                }

                return Packets.Dequeue();
            }
        }

        //处理数据包
        public void ProcessPackets()
        {
            Packet packet = null;

            for(packet = GetPacket(); packet != null; )
            {
                OnReceive handler = null;
                if(handlers.TryGetValue(packet.msgid, out handler))
                {
                    if(handler != null)
                    {
                        handler(packet);//调用相应的OnRecieve函数
                    }
                }

                packet = GetPacket();//处理其他包
            }
        }
    }
}

```
3.核心TCP网络功能
创建TCPPeer类封装TCP/IP协议的网络功能，它同时包括服务器监听和客户端连接的功能，无论是监听到客户端连接，还是连接到服务器，最后都会进入到接收数据的步骤。
因为TCP流不限定流的起始点和结束点，所以这里要通过变量readLength确认是否读满需要的数据长度。
```c#
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace UnityNetwork
{
    class TCPPeer
    {
        public enum MessageID //消息标识符
        {
            OnNewConnection = 1, //服务器接收新的连接
            OnConnected = 2, //连接到服务器
            OnConnectFail = 3, //无法连接到服务器
            OnDisconnect = 4, //失去远程的连接
        }
        protected MyEventHandler handler;//用于处理网络事件逻辑

        public TCPPeer(MyEventHandler h)
        {
            handler = h;
        }

        public void Listen(int port, int backlog = 128)
        {
            IPEndPoint ipe = new IPEndPoint(IPAddress.Any, port);//监听端口
            //创建服务器端TCPSocket
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Bind(ipe);//将socket绑定到端口上
                socket.Listen(backlog);//开始监听
                //异步接收连接
                socket.BeginAccept(new AsyncCallback(ListenCallback), socket);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        //服务器成功异步接收一个连接，并取得远程客户端的Socket
        void ListenCallback(IAsyncResult iar)
        {
            Socket listener = (Socket)iar.AsyncState;//取得服务器的Socket

            try
            {
                Socket client = listener.EndAccept(iar);//取得客户端的Socket

                //发布消息到逻辑队列
                handler.AddPacket(new Packet((short)MessageID.OnNewConnection, client));
                //创建接收数据的数据包
                Packet packet = new Packet(0, client);
                //开始接收来自客户端的数据
                client.BeginReceive(packet.buffer, 0, Packet.headerLength, SocketFlags.None, new AsyncCallback(ReceiveHeader), packet);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            //继续接收其他连接
            listener.BeginAccept(new AsyncCallback(ListenCallback), listener);
        }

        //作为TCP客户端，开始异步连接服务器
        public Socket Connect(string ip, int port)
        {
            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(ip), port);

            try
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //开始连接服务器
                socket.BeginConnect(ipe, new AsyncCallback(ConnectionCallback), socket);
                return socket;
            }
            catch(Exception e)
            {
                handler.AddPacket(new Packet((short)MessageID.OnConnectFail));
                Console.WriteLine(e.Message);
                return null;
            }
        }

        //客户端异步连接回调
        void ConnectionCallback(IAsyncResult iar)
        {
            Socket client = (Socket)iar.AsyncState;
            try
            {
                client.EndConnect(iar);//与服务器取得连接
                //通知已经成功连接到的服务器
                handler.AddPacket(new Packet((short)MessageID.OnConnected, client));
                //开始异步接收服务器信息
                Packet packet = new Packet(0, client);
                client.BeginReceive(packet.buffer, 0, Packet.headerLength, SocketFlags.None, new AsyncCallback(ReceiveHeader), packet);
            }
            catch(Exception e)
            {
                handler.AddPacket(new Packet((short)MessageID.OnConnectFail, client));
            }
        }

        //无论是创建用于监听的服务器Socket还是用于发起连接的客户端Socket
        //最后都会进入接收数据状态
        //接收数据主要是通过ReceiveHeader和ReceiveBody两个方法
        void ReceiveHeader(IAsyncResult iar)
        {
            Packet packet = (Packet)iar.AsyncState;

            try
            {
                int read = packet.sk.EndReceive(iar);//获取接收的数据长度
                if(read < 1) //小于1代表失去连接
                {
                    //通知失去连接
                    handler.AddPacket(new Packet((short)MessageID.OnDisconnect, packet.sk));
                    return;
                }

                packet.readLength += read;
                //消息头必须读满4个字节，如果未读满，继续读取剩下的数据
                if(packet.readLength < Packet.headerLength)
                {
                    packet.sk.BeginReceive(packet.buffer, packet.readLength, Packet.headerLength - packet.readLength, SocketFlags.None, new AsyncCallback(ReceiveHeader), packet);
                }
                else
                {
                    packet.DecodeHeader();//获取实际的数据长度
                    packet.readLength = 0;//重新记录读取的字节数量
                    //开始读取消息体
                    packet.sk.BeginReceive(packet.buffer, Packet.headerLength, packet.bodyLength, SocketFlags.None, new AsyncCallback(ReceiveBody), packet);
                }
            }
            catch(Exception e)
            {
                handler.AddPacket(new Packet((short)MessageID.OnDisconnect, packet.sk));
            }
        }

        //接收消息体
        void ReceiveBody(IAsyncResult iar)
        {
            Packet packet = (Packet)iar.AsyncState;

            try
            {
                int read = packet.sk.EndReceive(iar);//获取接受的数据流长度
                if(read < 1)
                {
                    handler.AddPacket(new Packet((short)MessageID.OnDisconnect, packet.sk));
                    return;
                }

                packet.readLength += read;

                if(packet.readLength < packet.bodyLength)
                {
                    packet.sk.BeginReceive(packet.buffer, Packet.headerLength + packet.readLength, packet.bodyLength - packet.readLength, SocketFlags.None, new AsyncCallback(ReceiveBody), packet);
                }
                else
                {
                    //复制读取的数据包，将其传入逻辑处理队列
                    Packet newpacket = new Packet(packet);
                    handler.AddPacket(newpacket);

                    //下一个读取，直到断开连接，读取的过程是一直在循环的
                    packet.ResetParams();
                    packet.sk.BeginReceive(packet.buffer, 0, Packet.headerLength, SocketFlags.None, new AsyncCallback(ReceiveHeader), packet);
                }
            }
            catch(Exception e)
            {
                handler.AddPacket(new Packet((short)MessageID.OnDisconnect, packet.sk));
            }
        }

        //向远程发送数据
        public static void Send(Socket sk, Packet packet)
        {
            if(!packet.encoded)
            {
                throw new Exception("Error Packet");
            }

            NetworkStream ns;

            lock(sk)
            {
                ns = new NetworkStream(sk);
                if(ns.CanWrite)
                {
                    try
                    {
                        ns.BeginWrite(packet.buffer, 0, Packet.headerLength + packet.bodyLength, new AsyncCallback(SendCallback), ns);
                    }
                    catch(Exception e)
                    {

                    }
                }
            }
        }

        //发送数据回调，主要是清理工作
        private static void SendCallback(IAsyncResult iar)
        {
            NetworkStream ns = (NetworkStream)iar.AsyncState;

            try
            {
                ns.EndWrite(iar);
                ns.Flush();
                ns.Close();
            }
            catch(Exception e)
            {

            }
        }
    }
}

```
4.创建聊天协议
```c#
using System;

namespace UnityNetwork
{
    //用于聊天的协议，包括一个用户名和聊天内容
    [Serializable]
    public struct ChatProto
    {
        public string userName; //用户名
        public string chatMsg; //聊天内容
    }
}
```

### Unity聊天客户端

1.在MyNetTest工程中新建一个Scene，将UnityNetwork.dll复制到工程中
2.创建脚本ChatHandler
```c#
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
```
3.创建脚本ChatClient
```c#
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
```
4.创建一个简单了聊天UI

### 聊天服务器端

1.在UnityNetwork工程中添加一个控制台工程ChatServer
2.在ChatServer上右键-Add-reference引入前面的那个网络库工程
3.创建一个ChatServer类，继承自NetworkManager
```c#
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using UnityNetwork;

namespace ChatServer
{
    class ChatServer : MyEventHandler
    {
        public enum MessageID
        {
            Chat = 100,//聊天消息标识符
        }

        private TCPPeer peer;//服务端
        private List<Socket> peerList;//保存所有的客户端连接
        private Thread thread;//逻辑线程
        private bool isRunning = false;//用于关闭线程的标志
        //用于暂停线程
        protected EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

        public ChatServer()
        {
            peerList = new List<Socket>();
        }

        //启动服务器
        public void RunServer(int port)
        {
            //添加事件
            AddHandler((short)TCPPeer.MessageID.OnNewConnection, OnNewConnection);
            AddHandler((short)TCPPeer.MessageID.OnDisconnect, OnDisconnected);
            AddHandler((short)MessageID.Chat, OnChat);

            peer = new TCPPeer(this);
            peer.Listen(port);

            isRunning = true;
            thread = new Thread(UpdateHandler);
            thread.Start();
            Console.WriteLine("Start Server");
        }

        //回调函数
        public void OnNewConnection(Packet packet)
        {
            Console.WriteLine("New Connection : {0}", packet.sk.RemoteEndPoint);
            peerList.Add(packet.sk);
        }
        public void OnDisconnected(Packet packet)
        {
            Console.WriteLine("Lost Connection : {0}", packet.sk.RemoteEndPoint);
            peerList.Remove(packet.sk);
        }
        //处理聊天消息
        public void OnChat(Packet packet)
        {
            string message = string.Empty;
            byte[] bs = null;

            using(MemoryStream stream = packet.Stream)
            {
                try
                {
                    BinaryReader br = new BinaryReader(stream);
                    int len = br.ReadInt32();//读的规则与客户端写一致
                    bs = br.ReadBytes(len);

                    ChatProto proto = Packet.Deserialize<ChatProto>(bs);
                    Console.WriteLine("{0} : {1}", proto.userName, proto.chatMsg);
                }
                catch
                {
                    return;//错误处理省略
                }
            }
            //准备发送的数据包
            Packet response = new Packet((short)MessageID.Chat);
            using(MemoryStream stream = response.Stream)
            {
                try
                {
                    BinaryWriter bw = new BinaryWriter(stream);
                    bw.Write(bs.Length);
                    bw.Write(bs);
                    response.EncodeHeader(stream);
                }
                catch
                {
                    return;
                }
            }
            //广播给所有客户端
            foreach(Socket sk in peerList)
            {
                TCPPeer.Send(sk, response);
            }
        }

        //重写
        public override void AddPacket(Packet packet)
        {
            lock(Packets)
            {
                Packets.Enqueue(packet);
                waitHandle.Set();//通知逻辑线程继续运行
            }
        }

        //这部分涉及到了线程的知识，之前在java课上学习过线程知识，但是没有自己写过
        //这里并不明白线程暂停的原理。

        //逻辑线程循环
        private void UpdateHandler()
        {
            while(isRunning)
            {
                waitHandle.WaitOne(-1);//直到等待新的信号
                //Thread.Sleep(30);//等待N毫秒更新一次
                ProcessPackets();
            }
            thread.Join();//回到主线程
            Console.WriteLine("Close Event Thread");
        }
    }
}

```

### JSON.NET简介

1.使用方式
```c#
using Newtonsoft.Json

[System.Serializable]
public struct ChatProto
{
	public string userName;
	public string chatMsg;
}
ChatProto proto =  new CharProto();

string json = JsonConvert.SerializeObject(chat);
proto = JsonConvert.DeserializeObject<ChatProto>(json);
```
2.BSON,基于二进制，效率更高
```c#
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

CHatProto chat = new ChatProto();

byte[] serializedData = new byte[]{};

using(var stream = new System.IO.MemoryStream())
{
	using(BsonWriter writer = new BsonWriter(stream))
	{
		JsonSerializer serializer = new JsonSerializer();
		serializer.Serialize(writer, chat);
	}
	serializedData = stream.ToArray();
}

using(var stream = new System.IO.MemoryStream(serializedData))
{
	using(BsonReader reader = new BsonReader(stream))
	{
		JsonSerializer serializer = new JsonSerializer();
		chat = serializer.Deserialize<ChatProto>(reader);
	}
}
```
3.Unity自带的JSON序列化
```c#
ChatProto chat = new ChatProto();

string json = JsonUtility.ToJson(chat);
ChatProto proto = JsonUtility.FromJson<ChatProto>(json);
```