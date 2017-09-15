using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Trace.Util
{
    public class TcpThreadMgr
    {
        //负责监听 客户端段 连接请求的  套接字
        private Socket sokWatch = null;

        //负责 调用套接字， 执行 监听请求的线程
        private Thread threadWatch = null;

        //是否在监听
        bool isWatch = true;

        public static Dictionary<string, ConnectionClient> dictConn = new Dictionary<string, ConnectionClient>();

        public void CreateTcpThread()
        {
            //实例化 套接字 （ip4寻址协议，流式传输，TCP协议）
            sokWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //获取本机的IP地址
            String strIPAddress = GetAddressIP();

            //创建 ip对象
            IPAddress address = IPAddress.Parse(strIPAddress);

            //创建网络节点对象 包含 ip和port
            IPEndPoint endpoint = new IPEndPoint(address, Constants.TCP_PORT);

            //将 监听套接字  绑定到 对应的IP和端口
            sokWatch.Bind(endpoint);

            //设置 监听队列 长度为10(同时能够处理 10个连接请求)
            sokWatch.Listen(10);

            threadWatch = new Thread(StartWatch);
            threadWatch.IsBackground = true;
            threadWatch.Start();
            System.Diagnostics.Trace.WriteLine("启动服务器成功......\r\n");
        }

        /// <summary>
        /// 被线程调用 监听连接端口
        /// </summary>
        private void StartWatch()
        {
            while (isWatch)
            {
                //监听 客户端 连接请求，但是，Accept会阻断当前线程
                Socket sokMsg = sokWatch.Accept();//监听到请求，立即创建负责与该客户端套接字通信的套接字
                
                ConnectionClient connection = new ConnectionClient(sokMsg, ShowMsg, RemoveClientConnection);
                
                //将负责与当前连接请求客户端 通信的套接字所在的连接通信类 对象 装入集合
                dictConn.Add(sokMsg.RemoteEndPoint.ToString(), connection);
                ShowMsg("接收连接成功......");
            }
        }

        /// <summary>
        /// 移除与指定客户端的连接
        /// </summary>
        /// <param name="key">指定客户端的IP和端口</param>
        public void RemoveClientConnection(string key)
        {
            if (dictConn.ContainsKey(key))
            {
                dictConn.Remove(key);
            }
        }

        /// <summary>
        /// 向文本框显示消息
        /// </summary>
        /// <param name="msgStr">消息</param>
        public void ShowMsg(string msgStr)
        {
            System.Diagnostics.Trace.WriteLine(msgStr);
        }

        #region 发送消息 到指定的客户端 -btnSend_Click
        //发送消息 到指定的客户端
        public void Send(byte[] cmdMsg)
        {
            if (cmdMsg.Length > 0)
            {
                foreach (ConnectionClient con in dictConn.Values)
                {
                    con.Send(cmdMsg);
                }
            }
            else
            {
                System.Diagnostics.Trace.WriteLine("请选择要发送的客户端～～");
            }
        }
        #endregion

        /// <summary>
        /// 获取本地IP地址信息
        /// </summary>
        private String GetAddressIP()
        {
            ///获取本地的IP地址
            string AddressIP = string.Empty;
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    AddressIP = _IPAddress.ToString();
                }
            }
            return AddressIP;
        }
    }
}
