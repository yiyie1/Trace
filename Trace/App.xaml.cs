using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace Trace
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //是否是没有PLC的情况下的调试状态
        public static bool isDebugWithoutPlc = false;

        public static Util.TcpThreadMgr tcpThreadMgr;

        //0管理员，1操作员
        private static Int32 userRole = -1;
        public static Int32 UserRole
        {
            get
            {
                return userRole;
            }
            set
            {
                userRole = value;
            }
        }

        public static string UserName;

        protected override void OnStartup(StartupEventArgs e)
        {
            //创建与PLC通讯的TCP线程
            tcpThreadMgr = new Util.TcpThreadMgr();
            tcpThreadMgr.CreateTcpThread();

            LoginWindow loginWin = new LoginWindow();
            loginWin.ShowDialog();
        }

    }
}
