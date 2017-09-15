using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Spectrograph;
using System.Threading;

namespace Trace
{
    /// <summary>
    /// Interaction logic for SysStateWindow.xaml
    /// </summary>
    public partial class SysStateWindow : Window
    {
        public SysStateWindow()
        {
            InitializeComponent();
        }

        private void RefreshState()
        {
            RecheckBtn_Click(null, null);
        }

        private bool CheckSystemState()
        {
            bool bPassingCheck1 = CheckSpectrographState();
            bool bPassingCheck2 = CheckControllerConnectionState();
            bool bPassingCheck3 = CheckControllerState();
            return bPassingCheck1 && bPassingCheck2 && bPassingCheck3;
        }

        private bool CheckControllerConnectionState()
        {
            bool res = Util.TcpThreadMgr.dictConn.Count > 0;
            if (res)
            {
                controllerConnectionCheckBox.IsChecked = true;
                stateTextBox.Text += "控制器通信正常\n";
            }
            else
            {
                String strError = "请查看控制器电源是否开启.\n";
                stateTextBox.Text += strError;
                MessageBox.Show(strError);
            }

            //return true;//todo

            return res;
        }

        private bool CheckControllerState()
        {
            Plc.Plc.STATESTRUCT state = Plc.Plc.CheckState();

            //避免发送check后太快发送home，接收端无法反应
            Thread.Sleep(500);

            bool isXok = (state.X_STAT != (UInt16)Plc.Plc.PLCState.AXIS_ERROR)
                && (state.X_STAT != (UInt16)Plc.Plc.PLCState.AXIS_OVERTIME);
            bool isYok = (state.Y_STAT != (UInt16)Plc.Plc.PLCState.AXIS_ERROR)
                && (state.Y_STAT != (UInt16)Plc.Plc.PLCState.AXIS_OVERTIME);
            bool isMok = (state.M_STAT != (UInt16)Plc.Plc.PLCState.AXIS_ERROR)
                && (state.M_STAT != (UInt16)Plc.Plc.PLCState.AXIS_OVERTIME);

            bool res = isXok && isYok && isMok;

            if (res)
            {
                controllerStateCheckBox.IsChecked = true;
                stateTextBox.Text += "控制器状态正常\n";

                if (state.X_STAT != (UInt16)Plc.Plc.PLCState.AXIS_HOMEDONE
                    || state.Y_STAT != (UInt16)Plc.Plc.PLCState.AXIS_HOMEDONE
                    || state.M_STAT != (UInt16)Plc.Plc.PLCState.AXIS_HOMEDONE)
                {
                    Thread thread = new Thread(Plc.Plc.Home);
                    thread.IsBackground = true;
                    thread.Start();
                }
            }
            else
            {
                String strError = String.Empty;

                if (state.X_STAT == (UInt16)Plc.Plc.PLCState.AXIS_ERROR)
                {
                    strError += "X轴存在故障\n";
                }

                if (state.Y_STAT == (UInt16)Plc.Plc.PLCState.AXIS_ERROR)
                {
                    strError += "Y轴存在故障\n";
                }

                if (state.M_STAT == (UInt16)Plc.Plc.PLCState.AXIS_ERROR)
                {
                    strError += "M轴存在故障\n";
                }

                if (state.X_STAT == (UInt16)Plc.Plc.PLCState.AXIS_OVERTIME)
                {
                    strError += "X轴连接失败\n";
                }

                if (state.Y_STAT == (UInt16)Plc.Plc.PLCState.AXIS_OVERTIME)
                {
                    strError += "Y轴连接失败\n";
                }

                if (state.M_STAT == (UInt16)Plc.Plc.PLCState.AXIS_OVERTIME)
                {
                    strError += "M轴连接失败\n";
                }

                stateTextBox.Text += strError;
                MessageBox.Show(strError);
            }

            //return true;//todo

            return res;
        }

        private bool CheckSpectrographState()
        {
            bool rst = true;
            int spectroGraphCount = Spectrograph.Spectrograph.GetSpectrographCount();
            if (spectroGraphCount >= 1)
            {
                spectrographCheckBox.IsChecked = true;
                stateTextBox.Text += "光谱仪通信正常\n";
                rst = true;
            }
            else
            {
                String strError = "请重新拔插光谱仪的USB接口进行排查\n";
                stateTextBox.Text += strError;
                MessageBox.Show(strError);
                rst = false;
            }
            //return true;//TODO

            return rst;
        }

        private void BtnCreateTask_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            NewProjectWindow newProject = new NewProjectWindow();
            newProject.ShowDialog();
        }

        private void GotoMainWindow()
        {
            this.Hide();
            MainWindowNew main = new MainWindowNew();
            main.Show();
        }

        private void RecheckBtn_Click(object sender, RoutedEventArgs e)
        {
            stateTextBox.Text = String.Empty;
            bool rst = CheckSystemState();
            BtnCheck.IsEnabled = !rst;
            BtnCreateTask.IsEnabled = rst;
        }

        private void SysStateWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RecheckBtn_Click(null, null);
        }

        protected override void OnClosed(EventArgs e)
        {
            App.Current.Shutdown();
        }
    }
}
