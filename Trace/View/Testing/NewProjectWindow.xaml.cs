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
using DataBase;
using Trace.Model;

namespace Trace
{
    /// <summary>
    /// NewProject.xaml 的交互逻辑
    /// </summary>
    public partial class NewProjectWindow : Window
    {
        SPAM.CCoSpectralMath spectralMath;
        SPAM.CCoCIEConstants cieConstants;
        SPAM.CCoCIEObserver cieObserver;
        SPAM.CCoIlluminant illuminant;

        public static TestTask TestTask = new TestTask();

        public NewProjectWindow()
        {
            InitializeComponent();

            spectralMath = new SPAM.CCoSpectralMath();
            cieConstants = spectralMath.createCIEConstantsObject();
        }

        private void NewProject_Loaded(object sender, RoutedEventArgs e)
        {
            InitObserver();
            InitIlluminant();
            cieConstants.Dispose();
            spectralMath.Dispose();

            taskNameBox.Focus();
        }

        private void InitObserver()
        {
            cieObserver = new SPAM.CCoCIEObserver();

            int observerCount = cieConstants.getNumberOfObservers();
            int defaultAngleIndex = -1;
            for (int i = 0; i < observerCount; i++)
            {
                cieObserver = cieConstants.getCIEObserverByIndex(i);

                if (String.Compare(cieObserver.toString(), Constants.DEFAULT_LIGHT, true) == 0)
                {
                    defaultAngleIndex = i;
                }
                watchAngleBox.Items.Add(cieObserver.toString());
            }

            if (defaultAngleIndex >= 0)
            {
                watchAngleBox.SelectedIndex = defaultAngleIndex;
            }
            else
            {
                watchAngleBox.SelectedIndex = 0;
            }
        }

        //初始化照明光源
        private void InitIlluminant()
        {
            illuminant = new SPAM.CCoIlluminant();

            int illuminantCount = cieConstants.getNumberOfIlluminants();
            int defaultLightIndex = -1;
            for (int i = 0; i < illuminantCount; i++)
            {
                illuminant = cieConstants.getIlluminantByIndex(i);
                lightBox.Items.Add(illuminant.toString());
                if (String.Compare(illuminant.toString(), Constants.DEFAULT_LIGHT, true) == 0)
                {
                    defaultLightIndex = i;
                }
            }

            if (defaultLightIndex >= 0)
            {
                lightBox.SelectedIndex = defaultLightIndex;
            }
            else
            {
                lightBox.SelectedIndex = 0;
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (taskNameBox.Text.Trim() == "")
            {
                MessageBox.Show("请输入任务名称");
            }
            else if (testGroupBox.Text.Trim() == "")
            {
                MessageBox.Show("请输入测试组");
            }
            else if (reelNumberBox.Text.Trim() == "")
            {
                MessageBox.Show("请输入幅号");
            }
            else
            {
                TestTask.TaskName = taskNameBox.Text.Trim();
                TestTask.GroupName = testGroupBox.Text.Trim();
                TestTask.ReelNumber = reelNumberBox.Text.Trim();
                TestTask.LightSource = lightBox.Text.Trim();
                TestTask.ObserveAngle = watchAngleBox.Text.Trim();

                /*bool ret = DatabaseConnection.SaveTaskToDatabase(_task);
                if (!ret)
                    MessageBox.Show("保存任务到数据库失败");*/

                this.Hide();
                ProjectParamWindow projectParam = new ProjectParamWindow();
                projectParam.Closed += paramClosed;
                projectParam.Show();
            }
            
        }

        private void paramClosed(object sender, EventArgs e)
        {
            this.ShowDialog();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        protected override void OnClosed(EventArgs e)
        {
            App.Current.Shutdown();
        }

    }
}

