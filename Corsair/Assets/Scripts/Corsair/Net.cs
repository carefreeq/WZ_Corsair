using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using UnityEngine;
namespace Corsair
{
    public enum NetType : byte
    {
        TCP,
        UDP,
    }
    public enum NetStatus : int
    {
        Null,
        Server,
        Client,
    }
    public enum NetMessageType : int
    {
        Warning = 1,
        Error,
        Data,
        Heartbeat,
        GainServer,
        SendServer,
        Connecting,
        ConnectPass,
    }

    public partial class Net
    {
        public static string PlayerName { get; set; }
        public static IPAddress IP { get; set; }
        public static int Port { get; set; }
        public static IPEndPoint LocalIPEndPoint { get { return new IPEndPoint(IP, Port); } }
        public static NetStatus Status { get; protected set; }
        public static int Timeout { get; set; }
        public static bool LogEnable { get; set; }
        protected static List<NetData> NetDatas { get; private set; }
        public static event Action<NetData> NetDataEvent;
        static Net()
        {
            PlayerName = "Player";
            Timeout = 1000000000;
            IP = GetLocalIP();
            Port = 5000;
            LogEnable = false;
            NetDatas = new List<NetData>();
            NetDataMgr.Enable = true;

        }
        public static bool CheckNet()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.LogError("网络未连接");
                return false;
            }
            return true;
        }
        private static IPAddress GetLocalIP()
        {
            try
            {
                IPAddress[] ps0 = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
                foreach (IPAddress p in ps0)
                {
                    if (p.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return p;
                    }
                }
                IPAddress[] ps1 = Dns.GetHostEntry(IPAddress.Any.ToString()).AddressList;
                foreach (IPAddress p in ps1)
                {
                    if (p.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return p;
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("GetLocalIP Error:" + e.Message);
            }
            return IPAddress.Any;
        }
        public static byte[] ObjectToByte(object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, obj);
                return ms.GetBuffer();
            }
        }
        public static object ByteToObject(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                BinaryFormatter bf = new BinaryFormatter();
                return bf.Deserialize(ms);
            }
        }

        [DisallowMultipleComponent]
        public class NetDataMgr : MonoBehaviour
        {
            private static bool enable = false;
            public static bool Enable
            {
                get { return enable; }
                set
                {
                    if (value != enable)
                    {
                        if (value)
                            mgr.AddComponent<NetDataMgr>();
                        else
                            Destroy(mgr.GetComponent<NetDataMgr>());
                        enable = value;
                    }
                }
            }
            private static GameObject mgr;
            static NetDataMgr()
            {
                mgr = new GameObject("NetManager");
                mgr.hideFlags = HideFlags.HideAndDontSave;
                DontDestroyOnLoad(mgr);
            }
            private void Update()
            {
                while (NetDatas.Count > 0)
                {
                    NetData n = NetDatas[0];
                    try
                    {
                        switch (n.MessageType)
                        {
                            // 1 下线警告
                            case NetMessageType.Warning:
                                Debug.LogWarning(n.ReadString());
                                break;
                            case NetMessageType.Error:
                                Debug.LogError(n.ReadString());
                                break;
                            case NetMessageType.Heartbeat:
                                int d = DateTime.Now.Millisecond - n.ReadInt();
                                Debug.Log(n.RemoteIP + "延迟:" + d);
                                break;
                            case NetMessageType.Data:
                                break;
                            case NetMessageType.GainServer:
                                if (Net.Status == NetStatus.Server)
                                {
                                    NetData g = new NetData(NetMessageType.SendServer);
                                    g.Write(NetServer.ServerName);
                                    g.Write(NetServer.PlayerName);
                                    g.Write(NetServer.IsPassword);
                                    g.Write(NetServer.ClientNumber);
                                    g.Write(NetServer.ClientMax);
                                    NetUdp.Send(g, n.RemoteIP);
                                }
                                break;
                            case NetMessageType.SendServer:
                                string ssn = n.ReadString();
                                string spn = n.ReadString();
                                IPEndPoint sip = n.RemoteIP;
                                bool spd = n.ReadBool();
                                int scn = n.ReadInt();
                                int scm = n.ReadInt();
                                NetClient.Servers.Add(new NetServer.Info(ssn, spn, sip, spd, scn, scm));
                                break;
                            case NetMessageType.Connecting:
                                if (Net.Status == NetStatus.Server)
                                {
                                    if (NetServer.ClientNumber >= NetServer.ClientMax)
                                    {
                                        NetData cw0 = new NetData(NetMessageType.Warning);
                                        cw0.Write("该服务器玩家数已满!");
                                        NetUdp.Send(cw0, n.RemoteIP);
                                    }
                                    string cn = n.ReadString();

                                    if (NetServer.IsPassword)
                                    {
                                        if (!n.IsRead())
                                        {
                                            NetData cw1 = new NetData(NetMessageType.Warning);
                                            cw1.Write("该服务器需要密码访问!");
                                            NetUdp.Send(cw1, n.RemoteIP);
                                            break;
                                        }

                                        string cp = n.ReadString();
                                        if (NetServer.Password != cp)
                                        {
                                            NetData cw2 = new NetData(NetMessageType.Warning);
                                            cw2.Write("该服务器访问密码错误!");
                                            NetUdp.Send(cw2, n.RemoteIP);
                                            break;
                                        }
                                    }
                                    if (NetServer.Clients.ContainsIP(n.RemoteIP))
                                    {
                                        NetData cw3 = new NetData(NetMessageType.Warning);
                                        cw3.Write("请勿重复登录!");
                                        NetUdp.Send(cw3, n.RemoteIP);
                                        break;
                                    }
                                    NetServer.Clients.Add(new NetClient.Info(cn, n.RemoteIP));
                                    NetData cw = new NetData(NetMessageType.ConnectPass);
                                    NetUdp.Send(cw, n.RemoteIP);
                                }
                                break;
                            case NetMessageType.ConnectPass:
                                NetClient.Connect(n.RemoteIP);
                                break;
                            default:
                                Debug.Log("未知数据包，来自:" + n.RemoteIP);
                                break;
                        }
                        n.Reset();
                        if (NetDataEvent != null)
                            NetDataEvent(n);
                        NetDatas.RemoveAt(0);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("数据包解析错误:" + e.Message.ToString());
                        NetDatas.RemoveAt(0);
                    }
                }
            }
        }
        protected abstract class NetInfo
        {
            internal delegate void CloseDelegate();
            internal event CloseDelegate CloseEvent;
            protected DataBuffer buffer = new DataBuffer();
            public virtual void Close()
            {
                if (CloseEvent != null)
                    CloseEvent();
            }
        }
        protected class DataBuffer
        {
            private byte[] data = new byte[0];
            public int DataLength { get { return (int)data.Length; } }
            private int readLength = -1;
            public int Readlength
            {
                get
                {
                    if (readLength < 0)
                        readLength = BitConverter.ToInt32(data, 0);
                    return readLength;
                }
            }
            public bool IsRead
            {
                get
                {
                    if (DataLength < 4 || Readlength < 0)
                        return false;
                    return DataLength >= Readlength;
                }
            }
            public void Write(byte[] d)
            {
                int l = data.Length;
                Array.Resize(ref data, data.Length + d.Length);
                Array.Copy(d, 0, data, l, d.Length);
            }
            public byte[] Read()
            {
                if (IsRead)
                {
                    byte[] b = new byte[Readlength];
                    Array.Copy(data, 0, b, 0, Readlength);
                    int l = DataLength - Readlength;
                    if (l > 0)
                    {
                        byte[] a = new byte[l];
                        Array.Copy(data, Readlength, a, 0, a.Length);
                        data = a;
                    }
                    else
                    {
                        data = new byte[0];
                    }
                    readLength = -1;
                    return b;
                }
                return null;
            }
        }
    }
    public partial class NetTcp : Net
    {
        protected static Socket socket;
        protected static Thread listen;
        protected class TcpInfo : NetInfo
        {
            internal IPEndPoint ip;
#if XRUWP && !UNITY_EDITOR
            private StreamSocket socket;
#else
            private Socket tcp;
#endif
#if XRUWP && !UNITY_EDITOR
            internal TcpInfo(StreamSocket socket)
            {
                ip = new IPEndPoint(IPAddress.Parse(socket.Information.RemoteAddress.ToString()),int.Parse(socket.Information.RemotePort));
#else
            public TcpInfo(Socket tcp)
            {
                ip = (IPEndPoint)tcp.RemoteEndPoint;
#endif
                this.tcp = tcp;
            }
            public void Send(byte[] data)
            {
#if XRUWP && !UNITY_EDITOR
                using (DataWriter dw = new DataWriter(socket.OutputStream))
                {
                    dw.WriteBytes(data);
                    await dw.StoreAsync();
                }
#else
                tcp.Send(data);
#endif
            }
            public void Receive()
            {
                Thread r = new Thread(() =>
                {
                    try
                    {
                        byte[] d = new byte[4096];
                        tcp.ReceiveTimeout = Net.Timeout;
                        int l = tcp.Receive(d);
                        while (l > 0)
                        {
                            if (LogEnable)
                                Debug.Log("收到" + ip + ",tcp数据长度:" + l);
                            if (l < d.Length)
                            {
                                byte[] _d = new byte[l];
                                Array.Copy(d, 0, _d, 0, l);
                                buffer.Write(_d);
                            }
                            else
                                buffer.Write(d);

                            if (buffer.IsRead)
                            {
                                NetData n = NetData.ReadBuffer(buffer.Read());
                                n.RemoteIP = ip;
                                Net.NetDatas.Add(n);
                            }
                            l = tcp.Receive(d);
                        }

                    }
                    catch (Exception e)
                    {
                        Debug.LogError("tcpInfo:" + ip + ":" + e.Message.ToString());
                    }
                    finally
                    {
                        if (LogEnable)
                            Debug.Log("tcp接收监听关闭");
                        Close();
                    }
                });
                r.Start();
                //心跳包线程
                Thread h = new Thread(() =>
                {
                    while (true)
                    {
                        Thread.Sleep(Timeout / 2);
                        if (tcp != null)
                        {
                            NetData n = new NetData(NetMessageType.Heartbeat);
                            n.Write(DateTime.Now.Millisecond);
                            Send(n.ToBuffer());
                        }
                        else
                            return;
                    }
                });
                h.Start();
            }
            public override void Close()
            {
                if (tcp != null)
                {
                    tcp.Close();
                    tcp = null;
                    base.Close();
                }
            }
        }
    }

    public sealed partial class NetServer : NetTcp
    {
        public class Info
        {
            public string ServerName { get; private set; }
            public string PlayerName { get; private set; }
            public IPEndPoint IP { get; private set; }
            public bool IsPassword { get; private set; }
            public int ClientMax { get; private set; }
            public int ClientNumber { get; private set; }
            public Info(string sn, string pn, IPEndPoint ip, bool pd, int cn, int cm)
            {
                ServerName = sn;
                PlayerName = pn;
                IP = ip;
                IsPassword = pd;
                ClientNumber = cn;
                ClientMax = cm;
            }
        }
        public static string ServerName { get; set; }
        public static string Password { get; set; }
        public static bool IsPassword { get { return Password != null; } }
        public static int ClientNumber { get { return Clients.Count; } }
        public static int ClientMax { get; private set; }
        public static List<NetClient.Info> Clients { get; set; }
        public static event Action<NetClient.Info> AddClientEvent;
        public static event Action<NetClient.Info> RemoveClientEvent;
        private static Dictionary<IPEndPoint, TcpInfo> clients = new Dictionary<IPEndPoint, TcpInfo>();
        static NetServer()
        {
            ServerName = "Server";
            Clients = new List<NetClient.Info>();
            clients = new Dictionary<IPEndPoint, TcpInfo>();
            Password = null;

        }
        public static void Listen(int listenNumber = 12)
        {
            if (!CheckNet())
                return;
            if (Status != NetStatus.Null)
            {
                Debug.LogError("请先退出Net状态:" + Status);
                return;
            }
            if (socket != null)
            {
                Debug.LogWarning("请勿重复创建tcp服务器");
            }
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(new IPEndPoint(IP, Port));
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                socket.Listen(listenNumber);
                ClientMax = listenNumber;
                listen = new Thread(() =>
                {
                    while (socket != null)
                    {
                        Socket s = socket.Accept();
                        IPEndPoint ip = (IPEndPoint)s.RemoteEndPoint;
                        if (Clients.ContainsIP(ip))
                        {
                            if (!clients.ContainsKey(ip))
                            {
                                TcpInfo n = new TcpInfo(s);
                                clients.Add(ip, n);
                                if (AddClientEvent != null)
                                    AddClientEvent(Clients.GetClient(ip));
                                n.Receive();
                                n.CloseEvent += () =>
                                {
                                    clients.Remove(ip);
                                    if (Clients.ContainsIP(ip))
                                    {
                                        if (RemoveClientEvent != null)
                                            RemoveClientEvent(Clients.GetClient(ip));
                                        Clients.Remove(ip);
                                    }
                                    Debug.Log(ip + "断开连接");
                                };
                                Debug.Log("新用户登入：" + ip);
                            }
                            else
                            {
                                s.Close();
                                Debug.Log("用户重复登入：" + ip);
                            }
                        }
                        else
                        {
                            s.Close();
                            string i = "";
                            foreach (var c in Clients)
                                i += c.PlayerName + " :" + c.IP + "\n";
                            Debug.Log("拒绝用户：" + ip + "\nClients:\n" + i);
                        }
                    }
                });
                listen.Start();
                Net.Status = NetStatus.Server;
                Debug.Log("tcp服务器开启，开始监听:" + socket.LocalEndPoint);
            }
            catch (Exception e)
            {
                socket = null;
                Debug.LogError("创建tcp服务器失败,错误：" + e.Message.ToString());
            }
        }
        //public static void Send(NetData data, NetType t = NetType.TCP)
        //{
        //    if (socket != null)
        //    {
        //        byte[] b = data.ToBuffer();
        //        if (LogEnable)
        //            Debug.Log("发送数据:" + b.Length);
        //        {
        //            switch (t)
        //            {
        //                case NetType.TCP:
        //                    foreach (var c in clients.Values)
        //                        c.Send(b);
        //                    break;
        //                case NetType.UDP:
        //                    foreach (var c in clients)
        //                        NetUdp.Send(b, c.Key);
        //                    break;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        Debug.LogError("未开启tcp服务器");
        //    }
        //}
        public static void Send(NetData data, NetType t = NetType.TCP, IPEndPoint ignore = null)
        {
            if (socket != null)
            {
                byte[] b = data.ToBuffer();
                if (LogEnable)
                    Debug.Log("发送数据:" + b.Length);
                {
                    switch (t)
                    {
                        case NetType.TCP:
                            foreach (var c in clients)
                                if (ignore == null || c.Key.ToString() != ignore.ToString())
                                    c.Value.Send(b);
                            break;
                        case NetType.UDP:
                            foreach (var c in clients)
                                if (ignore == null || c.Key.ToString() != ignore.ToString())
                                    NetUdp.Send(b, c.Key);
                            break;
                    }
                }
            }
            else
            {
                Debug.LogError("未开启tcp服务器");
            }
        }
        public static void SendTo(NetData data, IPEndPoint ip, NetType t = NetType.TCP)
        {
            if (clients.ContainsKey(ip))
            {
                switch (t)
                {
                    case NetType.TCP:
                        clients[ip].Send(data.ToBuffer());
                        break;
                    case NetType.UDP:
                        NetUdp.Send(data.ToBuffer(), ip);
                        break;
                }
            }
            else
            {
                Debug.LogError(ip + "未与本机建立连接");
            }
        }
        public static void ShutDown()
        {
            if (socket != null)
            {
                TcpInfo[] c = clients.Values.ToArray();
                for (int i = 0; i < c.Length; i++)
                {
                    c[i].Close();
                }
                socket.Close();
                socket = null;
                listen.Abort();
                Net.Status = NetStatus.Null;
                Debug.Log("tcp服务器关闭");
            }
            else
            {
                Debug.LogWarning("tcp服务器未开启");
            }
        }
        public static void ShutDown(IPEndPoint ip)
        {
            if (clients.ContainsKey(ip))
            {
                clients[ip].Close();
                clients.Remove(ip);
            }
        }
    }
    public sealed partial class NetClient : NetTcp
    {
        public class Info
        {
            public string PlayerName { get; private set; }
            public IPEndPoint IP { get; private set; }
            public Info(string p, IPEndPoint i)
            {
                PlayerName = p;
                IP = i;
            }
        }
        public static List<NetServer.Info> Servers { get; private set; }

        static NetClient()
        {
            Servers = new List<NetServer.Info>();
        }

        private static TcpInfo info;
        public static void Flush()
        {
            Servers.Clear();
            NetData n = new NetData(NetMessageType.GainServer);
            NetUdp.SendBroad(n);
        }
        public static void ConnectToServer(IPEndPoint ip, string password = null)
        {
            if (Status != NetStatus.Null)
            {
                Debug.LogError("请先退出Net状态:" + Status);
                return;
            }
            NetData n = new NetData(NetMessageType.Connecting);
            n.Write(PlayerName);
            if (password != null)
                n.Write(password);
            NetUdp.Send(n, ip);
        }
        public static void Connect(IPEndPoint ip)
        {
            if (!CheckNet())
                return;
            if (Status != NetStatus.Null)
            {
                Debug.LogError("请先退出Net状态:" + Status);
                return;
            }
            if (info != null)
            {
                Debug.LogWarning("请勿重复连接tcp服务器");
            }
            else
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(new IPEndPoint(IP, Port));
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                listen = new Thread(() =>
                {
                    try
                    {
                        Debug.Log("开始连接tcp服务器……");
                        socket.Connect(ip);
                        info = new TcpInfo(socket);
                        info.CloseEvent += () =>
                        {
                            info = null;
                            Net.Status = NetStatus.Null;
                            Debug.Log("断开tcp服务器");
                        };
                        info.Receive();
                        Net.Status = NetStatus.Client;
                        Debug.Log("成功连接tcp服务器：" + socket.RemoteEndPoint.ToString());
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("连接tcp服务器失败,提示：" + e.Message.ToString());
                    }
                });
                listen.Start();
            }
        }
        public static void Send(NetData data, NetType t = NetType.TCP)
        {
            if (info != null)
            {
                switch (t)
                {
                    case NetType.TCP:
                        info.Send(data.ToBuffer());
                        break;
                    case NetType.UDP:
                        NetUdp.Send(data.ToBuffer(), info.ip);
                        break;
                }
            }
            else
            {
                Debug.LogWarning("未连接tcp服务器");
            }
        }
        public static void ShutDown()
        {
            if (socket != null)
            {
                listen.Abort();
                if (info != null)
                {
                    info.Close();
                    info = null;
                }
                socket = null;
                Net.Status = NetStatus.Null;
            }
            else
            {
                Debug.LogWarning("未连接服务器");
            }
        }
    }
    public sealed class NetUdp : Net
    {
        private class UdpInfo : NetInfo
        {
#if XRUWP && !UNITY_EDITOR
            private DatagramSocket udp;
#else
            private IPEndPoint ip;
            private Socket udp;
            private Thread receive;
#endif
            public UdpInfo(IPEndPoint ip)
            {

#if XRUWP && !UNITY_EDITOR
                this.udp = new DatagramSocket();
#else

                this.ip = ip;
                //this.udp = new UdpClient(ip);

                this.udp = new Socket(ip.Address.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                this.udp.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                this.udp.Bind(ip);
#endif
            }
            private const int MAX = 65500;
            public void Send(byte[] data, IPEndPoint ip)
            {

                if (LogEnable)
                    Debug.Log("向" + ip + "发送udp数据:" + data.Length);
                while (data.Length > MAX)
                {
                    byte[] d = new byte[MAX];
                    Array.Copy(data, 0, d, 0, d.Length);
                    udp.SendTo(d, ip);
                    d = new byte[data.Length - d.Length];
                    Array.Copy(data, MAX, d, 0, d.Length);
                    data = d;
                }
                udp.SendTo(data, ip);
            }
            public void Receive()
            {
#if XRUWP && !UNITY_EDITOR
            async void receive()
            {
                DatagramSocket udp = new DatagramSocket();
                await udp.BindServiceNameAsync(ip.Port.ToString());
                udp.MessageReceived += async (s, r) =>
                {
                    using (DataReader dr = r.GetDataReader())
                    {
                        byte[] d = new byte[dr.UnconsumedBufferLength];
                        dr.ReadBytes(d);
                        if (Net.LogEnable)
                            Debug.Log("收到" + _ip + ",udp数据长度:" + d.Length);
                        buffer.Write(d);
                        if (buffer.IsRead())
                        {
                            NetData n = XRMaker.NetData.ReadBuffer(buffer.Read());
                            n.RemoteIP = _ip;
                            Net.NetData.Add(n);
                        }
                    }
                };
            }
            receive();
#else
                receive = new Thread(() =>
                {
                    EndPoint _ip = new IPEndPoint(IPAddress.Any, 0);
                    byte[] d = new byte[4096];
                    while (true)
                    {
                        try
                        {
                            int l = udp.ReceiveFrom(d, ref _ip);
                            if (LogEnable)
                                Debug.Log("收到" + _ip + ",udp数据长度:" + l);
                            if (l < d.Length)
                            {
                                byte[] _d = new byte[l];
                                Array.Copy(d, 0, _d, 0, l);
                                buffer.Write(_d);
                            }
                            else
                                buffer.Write(d);
                            if (buffer.IsRead)
                            {
                                NetData n = NetData.ReadBuffer(buffer.Read());
                                n.RemoteIP = (IPEndPoint)_ip;
                                Net.NetDatas.Add(n);
                            }
                        }
                        catch (Exception e)
                        {

                            Debug.LogError("udpInfo:" + e.Message.ToString());
                        }
                    }
                });
                receive.Start();
#endif
            }
            public override void Close()
            {
                if (udp != null)
                {
#if XRUWP && !UNITY_EDITOR
                    await udp.CancelIOAsync();
                    udp.Dispose();
#else
                    receive.Abort();
                    udp.Close();
#endif
                    udp = null;
                    base.Close();
                }
            }
        }
        private static UdpInfo info;
        private static IPAddress broadcast;
        static NetUdp()
        {
            broadcast = GetBroadcast();
        }
        private static IPAddress GetBroadcast()
        {
            if (IPGlobalProperties.GetIPGlobalProperties() != null)
            {
                NetworkInterface[] ns = NetworkInterface.GetAllNetworkInterfaces();
                if (ns != null && ns.Length > 0)
                {
                    foreach (NetworkInterface n in ns)
                    {
                        if (n.NetworkInterfaceType == NetworkInterfaceType.Loopback
                            || n.NetworkInterfaceType == NetworkInterfaceType.Unknown
                            || !n.Supports(NetworkInterfaceComponent.IPv4)
                            || n.OperationalStatus != OperationalStatus.Up)
                            continue;
                        foreach (UnicastIPAddressInformation u in n.GetIPProperties().UnicastAddresses)
                        {
                            if (u.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                byte[] p = u.Address.GetAddressBytes();
                                byte[] m = u.IPv4Mask.GetAddressBytes();
                                byte[] b = new byte[p.Length];
                                for (int i = 0; i < b.Length; i++)
                                {
                                    b[i] = (byte)(p[i] | (m[i] ^ 255));
                                }
                                return new IPAddress(b);
                            }
                        }
                    }
                }
            }
            return IPAddress.Broadcast;
        }
        public static void Listen()
        {
            if (!CheckNet())
                return;
            if (info != null)
            {
                Debug.LogWarning("请勿重复开启udp服务器");
            }
            else
            {
#if XRUWP && !UNITY_EDITOR
                info = new UdpInfo(Port.ToString());
#else
                IPEndPoint ip = new IPEndPoint(IPAddress.Any, Port);
                info = new UdpInfo(ip);
#endif
                info.Receive();
                Debug.Log("upd服务器开启，开始监听:" + ip);
            }
        }

        public static void SendBroad(NetData data)
        {
            Send(data, new IPEndPoint(broadcast, Port));
        }
        public static void Send(NetData data, IPEndPoint ip)
        {
            Send(data.ToBuffer(), ip);
        }
        public static void Send(byte[] data, IPEndPoint ip)
        {
            if (info != null)
            {
                info.Send(data, ip);
            }
            else
            {
                ////
                Socket udp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                udp.SendTo(data, ip);
                Debug.Log("向" + ip + "发送udp数据:" + data.Length);
            }
        }
        public static void ShutDown()
        {
            if (info != null)
            {
                info.Close();
                info = null;
                Debug.Log("关闭udp服务器成功");
            }
            else
            {
                Debug.LogWarning("重复udp服务器");
            }
        }
    }
    /// <summary>
    /// 数据包
    /// </summary>
    public struct NetData
    {
        public IPEndPoint RemoteIP { get; set; }
        public NetMessageType MessageType { get; set; }
        private byte[] data;
        public byte[] Data { get { return data; } }
        public int Index { get; private set; }
        public NetData(NetMessageType t)
        {
            RemoteIP = null;
            MessageType = t;
            data = new byte[0];
            Index = 0;
        }
        public void Reset()
        {
            Index = 0;
        }
        public bool IsRead()
        {
            return Index < Data.Length;
        }
        internal byte[] ToBuffer()
        {
            byte[] d = new byte[data.Length + 8];
            Array.Copy(BitConverter.GetBytes((int)d.Length), 0, d, 0, 4);
            Array.Copy(BitConverter.GetBytes((int)MessageType), 0, d, 4, 4);
            Array.Copy(data, 0, d, 8, data.Length);
            return d;
        }
        public static NetData ReadBuffer(byte[] d)
        {
            int l = BitConverter.ToInt32(d, 0);
            if (l == d.Length)
            {
                int t = BitConverter.ToInt32(d, 4);
                NetData n = new NetData((NetMessageType)t);
                n.data = new byte[d.Length - 8];
                Array.Copy(d, 8, n.data, 0, n.data.Length);
                return n;
            }
            else
                throw new Exception("data's length is error!");
        }
        private void WriteBuffer(params byte[] b)
        {
            int l = data.Length;
            Array.Resize(ref data, data.Length + b.Length);
            Array.Copy(b, 0, data, l, b.Length);
        }
        private byte[] ReadBuffer(uint len)
        {
            byte[] _b = PeekBuffer(len);
            Index += _b.Length;
            return _b;
        }
        private byte[] PeekBuffer(uint len)
        {
            byte[] _b = new byte[len];
            Array.Copy(data, Index, _b, 0, len);
            return _b;
        }
        public void Write(byte[] val)
        {
            WriteBuffer(BitConverter.GetBytes((uint)val.Length));
            WriteBuffer(val);
        }
        public byte[] PeekBytes()
        {
            uint l = BitConverter.ToUInt32(PeekBuffer(4), 0);
            byte[] a = PeekBuffer(l + 4);
            byte[] b = new byte[l];
            Array.Copy(a, 4, b, 0, l);
            return b;
        }
        public byte[] ReadBytes()
        {
            uint l = BitConverter.ToUInt32(ReadBuffer(4), 0);
            return ReadBuffer(l);
        }
        public void Write(int val)
        {
            WriteBuffer(BitConverter.GetBytes(val));
        }

        public int PeekInt()
        {
            return BitConverter.ToInt32(PeekBuffer(4), 0);
        }
        public int ReadInt()
        {
            return BitConverter.ToInt32(ReadBuffer(4), 0);
        }
        public void Write(bool val)
        {
            WriteBuffer(BitConverter.GetBytes(val));
        }
        public bool PeekBool()
        {
            return BitConverter.ToBoolean(PeekBuffer(1), 0);
        }
        public bool ReadBool()
        {
            return BitConverter.ToBoolean(ReadBuffer(1), 0);
        }
        public void Write(float val)
        {
            WriteBuffer(BitConverter.GetBytes(val));
        }
        public float PeekFloat()
        {
            return BitConverter.ToSingle(PeekBuffer(4), 0);
        }
        public float ReadFloat()
        {
            return BitConverter.ToSingle(ReadBuffer(4), 0);
        }
        public void Write(float[] val)
        {
            byte[] d = new byte[val.Length * 4];
            for (int i = 0; i < val.Length; i++)
            {
                byte[] _d = BitConverter.GetBytes(val[i]);
                int _i = i * 4;
                d[_i] = _d[0];
                d[_i + 1] = _d[1];
                d[_i + 2] = _d[2];
                d[_i + 3] = _d[3];
            }
            Write(d);
        }
        public float[] PeekFloats()
        {
            byte[] d = PeekBytes();
            float[] f = new float[d.Length / 4];
            for (int i = 0; i < f.Length; i++)
            {
                f[i] = BitConverter.ToSingle(d, i * 4);
            }
            return f;
        }
        public float[] ReadFloats()
        {
            byte[] d = ReadBytes();
            float[] f = new float[d.Length / 4];
            for (int i = 0; i < f.Length; i++)
            {
                f[i] = BitConverter.ToSingle(d, i * 4);
            }
            return f;
        }

        public void Write(Vector3 val)
        {
            WriteBuffer(BitConverter.GetBytes(val.x));
            WriteBuffer(BitConverter.GetBytes(val.y));
            WriteBuffer(BitConverter.GetBytes(val.z));
        }
        public Vector3 PeekVector3()
        {
            byte[] b = PeekBuffer(12);
            return new Vector3(BitConverter.ToSingle(b, 0), BitConverter.ToSingle(b, 4), BitConverter.ToSingle(b, 8));
        }
        public Vector3 ReadVector3()
        {
            return new Vector3(ReadFloat(), ReadFloat(), ReadFloat());
        }
        public void Write(Quaternion val)
        {
            WriteBuffer(BitConverter.GetBytes(val.x));
            WriteBuffer(BitConverter.GetBytes(val.y));
            WriteBuffer(BitConverter.GetBytes(val.z));
            WriteBuffer(BitConverter.GetBytes(val.w));
        }
        public Quaternion PeekQuaternion()
        {
            byte[] b = PeekBuffer(12);
            return new Quaternion(BitConverter.ToSingle(b, 0), BitConverter.ToSingle(b, 4), BitConverter.ToSingle(b, 8), BitConverter.ToSingle(b, 12));
        }
        public Quaternion ReadQuaternion()
        {
            return new Quaternion(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());
        }

        public void Write(Color val)
        {
            byte[] col = new byte[] { (byte)(val.r * 255), (byte)(val.g * 255), (byte)(val.b * 255), (byte)(val.a * 255) };
            WriteBuffer(col);
        }
        public Color PeekColor()
        {
            byte[] b = PeekBuffer(4);
            return new Color(b[0] / 255f, b[1] / 255f, b[2] / 255f, b[3] / 255f);
        }
        public Color ReadColor()
        {
            byte[] b = ReadBuffer(4);
            return new Color(b[0] / 255f, b[1] / 255f, b[2] / 255f, b[3] / 255f);
        }
        public void Write(byte b)
        {
            WriteBuffer(b);
        }
        public byte PeekByte()
        {
            return PeekBuffer(1)[0];
        }
        public byte ReadByte()
        {
            return ReadBuffer(1)[0];
        }
        public void Write(string val)
        {
            byte[] b = Encoding.UTF8.GetBytes(val);
            WriteBuffer(BitConverter.GetBytes((ushort)b.Length));
            WriteBuffer(b);
        }
        public string PeekString()
        {
            ushort l = BitConverter.ToUInt16(PeekBuffer(2), 0);
            return Encoding.UTF8.GetString(PeekBuffer((uint)l + 2), 2, l);
        }
        public string ReadString()
        {
            ushort l = BitConverter.ToUInt16(ReadBuffer(2), 0);
            return Encoding.UTF8.GetString(ReadBuffer(l), 0, l);
        }

        public void Write(IPEndPoint ip)
        {
            Write(ip.Address.ToString());
            Write(ip.Port);
        }
        public IPEndPoint PeekIPEndPoint()
        {
            return new IPEndPoint(IPAddress.Parse(PeekString()), PeekInt());
        }
        public IPEndPoint ReadIPEndPoint()
        {
            return new IPEndPoint(IPAddress.Parse(ReadString()), ReadInt());
        }
    }
    public static class NetClientInfoExtension
    {
        public static NetClient.Info GetClient(this List<NetClient.Info> infos, IPEndPoint ip)
        {
            foreach (var i in infos)
            {
                if (i.IP.ToString() == ip.ToString())
                    return i;
            }
            return null;
        }
        public static bool ContainsIP(this List<NetClient.Info> infos, IPEndPoint ip)
        {
            foreach (var i in infos)
            {
                if (i.IP.ToString() == ip.ToString())
                    return true;
            }
            return false;
        }
        public static void Remove(this List<NetClient.Info> infos, IPEndPoint ip)
        {
            for (int i = 0; i < infos.Count; i++)
            {
                if (infos[i].IP.ToString() == ip.ToString())
                    infos.RemoveAt(i);
            }
        }
    }
}