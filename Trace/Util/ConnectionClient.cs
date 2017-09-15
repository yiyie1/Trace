using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;

/// <summary>
/// 与客户端的 连接通信类(包含了一个 与客户端 通信的 套接字，和线程)
/// </summary>
/// 
namespace Trace.Util
{
    public delegate void DGShowMsg(string strMsg);
    public class ConnectionClient
    {
        Socket sokMsg;
        DGShowMsg dgShowMsg;//负责 向主窗体文本框显示消息的方法委托
        DGShowMsg dgRemoveConnection;// 负责 从主窗体 中移除 当前连接
        Thread threadMsg;

        #region 构造函数
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sokMsg">通信套接字</param>
        /// <param name="dgShowMsg">向主窗体文本框显示消息的方法委托</param>
        public ConnectionClient(Socket sokMsg, DGShowMsg dgShowMsg, DGShowMsg dgRemoveConnection)
        {
            this.sokMsg = sokMsg;
            this.dgShowMsg = dgShowMsg;
            this.dgRemoveConnection = dgRemoveConnection;

            System.Diagnostics.Trace.WriteLine("建立监听线程");

            this.threadMsg = new Thread(RecMsg);
            this.threadMsg.IsBackground = true;
            this.threadMsg.Start();
        }
        #endregion

        bool isRec = true;
        #region 负责监听客户端发送来的消息
        void RecMsg()
        {
            while (isRec)
            {
                try
                {
                    byte[] stateInfo = new byte[1024 * 1024 * 2];
                    
                    //接收 对应 客户端发来的消息
                    int length = sokMsg.Receive(stateInfo);

                    Plc.Plc.STATESTRUCT state = (Plc.Plc.STATESTRUCT)BytesStructConvert.BytesToStuct(stateInfo, typeof(Plc.Plc.STATESTRUCT));
                    Plc.Plc.SetState(state);

                    System.Diagnostics.Trace.WriteLine("Receive TCP MSG: SYS_STAT=" + state.SYS_STAT.ToString("x")
                        + ", X_STAT=" + state.X_STAT.ToString("x") + ", Y_STAT=" + state.Y_STAT.ToString("x") + ", M_STAT="
                        + state.M_STAT.ToString("x") + ", CMD_SERIAL=" + state.CMD_SERIAL.ToString("x"));
                }
                catch (Exception ex)
                {
                    isRec = false;
                    
                    //移除 集合中对应的 ConnectionClient对象
                    dgRemoveConnection(sokMsg.RemoteEndPoint.ToString());
                }
            }
        }
        #endregion

        #region 向客户端发送消息
        /// <summary>
        /// 向客户端发送消息
        /// </summary>
        /// <param name="strMsg"></param>
        public void Send(byte[] cmdMsg)
        {
            sokMsg.Send(cmdMsg);
        }
        #endregion

        #region 关闭与客户端连接
        /// <summary>
        /// 关闭与客户端连接
        /// </summary>
        public void CloseConnection()
        {
            isRec = false;
        }
        #endregion
    }
}
