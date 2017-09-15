using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Collections;
using System.Windows.Controls.DataVisualization.Charting;
using System.Threading;
using System.Windows.Threading;
using Trace.Model;
using Plc;
using Spectrograph;

namespace Trace
{
    /// <summary>
    /// AjustWin.xaml 的交互逻辑
    /// </summary>
    public partial class AjustWindow : Window
    {
        #region 变量

        //画光谱图时横坐标的间隔
        private const int WRITE_PIC_INVERVEL = 4;

        //连续扫描的时间间隔
        private const int SCAN_TIME_INTERVEL = 5000000;

        //校准的参数
        public static List<AjustParam> ajustParamList = new List<AjustParam>();

        //弹出的提示窗口
        public static MsgBox msgBox;

        //打開提示窗口的委托
        private delegate void OpenMsgBox();
        private OpenMsgBox openMsgBox;

        //关闭提示窗口的委托
        private delegate void CloseMsgBox();
        private CloseMsgBox closeMsgBox;

        //提示用户调整光阑的委托
        private delegate void ImformUserModifyStop(int stop);
        private ImformUserModifyStop imformUserModifyStop;

        //更新界面按鈕
        private delegate void UpdateButtonsDelegate();
        private UpdateButtonsDelegate updateButtonsDelegate;

        //等待点击保存参考光谱按键信号
        static AutoResetEvent areSaveRef = new AutoResetEvent(false);

        //等待点击保存暗光谱按键信号
        static AutoResetEvent areSaveDark = new AutoResetEvent(false);

        //计时器，间隔时间进行画图
        DispatcherTimer timerOfPic = new DispatcherTimer();

        //暗灯图片路径
        private const String ANDENG_PIC_PATH = "/Resource/Image/andeng.png";

        //亮灯图片路径
        private const String LIANGDENG_PIC_PATH = "/Resource/Image/liangdeng.png";

        //当前处理到的校准参数Index
        int currentAjustIndex = 0;

        //更新按键的类
        SetButtonThread updateBtnsThread;

        //当前光谱及各参数
        private double[] pixels;
        private int currentIntegrationTime = Constants.DEFAULT_INTEGRATION_TIME;
        private int currentAverage = Constants.DEFAULT_AVRAGE_TIMES;
        private int currentBoxcarWidth = Constants.DEFAULT_BOXCAR_WIDTH;

        //更新图片的委托
        delegate void UpdatePicDelegate();
        private UpdatePicDelegate updatePicDelegate;

        //画图线程
        private Thread thTemp;

        //是否当前处于持续测量并画图的状态
        private bool isContinueUpdatingPic = true;

        //积分时间
        public static int GetIntegrationTime(ComboBox cBUnit, TextBox tBIntegral)
        {
            int usIntegrateTime = Constants.DEFAULT_INTEGRATION_TIME;
            try
            {
                if (cBUnit.SelectedIndex == 0)
                {
                    usIntegrateTime = int.Parse(tBIntegral.Text) * 1000;
                }
                else if (cBUnit.SelectedIndex == 1)
                {
                    usIntegrateTime = int.Parse(tBIntegral.Text) * 1000000;
                }

                if (usIntegrateTime >= Constants.LARGEST_INTEGRATION_TIME)
                {
                    usIntegrateTime = Constants.LARGEST_INTEGRATION_TIME;
                }
                else if (usIntegrateTime <= Constants.SMALLEST_INTEGRATION_TIME)
                {
                    usIntegrateTime = Constants.SMALLEST_INTEGRATION_TIME;
                }

                return usIntegrateTime;
            }
            catch
            {
                return usIntegrateTime;
            }
        }

        public static int GetAverage(TextBox tBAverTimes)
        {
            int average = int.Parse(tBAverTimes.Text);

            if (average <= Constants.SMALLEST_AVRAGE_TIMES)
            {
                average = Constants.SMALLEST_AVRAGE_TIMES;
            }
            else if (average >= Constants.LARGEST_AVERAGE_TIMES)
            {
                average = Constants.LARGEST_AVERAGE_TIMES;
            }

            return average;
        }

        public static int GetBoxcarWidth(TextBox tBSmooth)
        {
            int boxcarWidth = int.Parse(tBSmooth.Text);

            if (boxcarWidth <= Constants.SMALLEST_BOXCAR_WIDTH)
            {
                boxcarWidth = Constants.SMALLEST_BOXCAR_WIDTH;
            }
            else if (boxcarWidth >= Constants.LARGEST_BOXCAR_WIDTH)
            {
                boxcarWidth = Constants.LARGEST_BOXCAR_WIDTH;
            }

            return boxcarWidth;
        }

        //测试使用
        //private static List<Model.ProjectParam> projectParamList = new List<Model.ProjectParam>();
        //public static List<Model.ProjectParam> ProjectParamList
        //{
        //    get
        //    {
        //        if (projectParamList.Count == 0)
        //        {
        //            Model.ProjectParam projectParam = new Model.ProjectParam();
        //            projectParam.OperID = 1;
        //            projectParam.PositionX = 20;
        //            projectParam.PositionY = 20;
        //            projectParam.Angle = 30;
        //            projectParam.Stop = 8;
        //            projectParamList.Add(projectParam);

        //            projectParam = new Model.ProjectParam();
        //            projectParam.OperID = 1;
        //            projectParam.PositionX = 20;
        //            projectParam.PositionY = 20;
        //            projectParam.Angle = 30;
        //            projectParam.Stop = 8;
        //            projectParamList.Add(projectParam);

        //            projectParam = new Model.ProjectParam();
        //            projectParam.OperID = 1;
        //            projectParam.PositionX = 25;
        //            projectParam.PositionY = 25;
        //            projectParam.Angle = 25;
        //            projectParam.Stop = 6;
        //            projectParamList.Add(projectParam);

        //            projectParam = new Model.ProjectParam();
        //            projectParam.OperID = 1;
        //            projectParam.PositionX = 15;
        //            projectParam.PositionY = 15;
        //            projectParam.Angle = 25;
        //            projectParam.Stop = 8;
        //            projectParamList.Add(projectParam);

        //            projectParam = new Model.ProjectParam();
        //            projectParam.OperID = 1;
        //            projectParam.PositionX = 10;
        //            projectParam.PositionY = 10;
        //            projectParam.Angle = 15;
        //            projectParam.Stop = 2;
        //            projectParamList.Add(projectParam);

        //            projectParam = new Model.ProjectParam();
        //            projectParam.OperID = 1;
        //            projectParam.PositionX = 10;
        //            projectParam.PositionY = 10;
        //            projectParam.Angle = 10;
        //            projectParam.Stop = 2;
        //            projectParamList.Add(projectParam);

        //            return projectParamList;
        //        }
        //        else
        //        {
        //            return projectParamList;
        //        }
        //    }
        //}

        #endregion

        public AjustWindow()
        {
            InitializeComponent();

            closeMsgBox = new CloseMsgBox(CloseMsgWin);
            imformUserModifyStop = new ImformUserModifyStop(ImformUserToModifyStopMethod);
            openMsgBox = new OpenMsgBox(OpenMsgBoxMethod);
            updatePicDelegate = new UpdatePicDelegate(UpdatePicMethod);

            timerOfPic.Tick += new EventHandler(timerOfPic_Tick);
            timerOfPic.Interval = new TimeSpan(SCAN_TIME_INTERVEL);
            CbUnit.SelectionChanged += CbUnit_SelectionChanged;

            this.Closed += AjustWindow_Closed;
        }

        void AjustWindow_Closed(object sender, EventArgs e)
        {
            StopContinueScan();

            TestResultChart.Axes.Clear();
            TestResultChart.Series.Clear();
        }

        void CbUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbUnit.SelectedIndex == 0)
            {
                tBIntegral.Text = "100";
            }
            else if (CbUnit.SelectedIndex == 1)
            {
                tBIntegral.Text = "1";
            }
        }

        #region 限定TextBox输入数字

        private void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!isNumberic(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        //isDigit是否是数字
        public static bool isNumberic(string _string)
        {
            if (string.IsNullOrEmpty(_string))
            {
                return false;
            }

            foreach (char c in _string)
            {
                if (!char.IsDigit(c))
                {
                    //if(c<'0' c="">'9')//最好的方法,在下面测试数据中再加一个0，然后这种方法效率会搞10毫秒左右
                    return false;
                }
            }
            return true;
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!isNumberic(e.Text))
            {
                e.Handled = true;
            }
            else
            {
                e.Handled = false;
            }
        }

        #endregion

        private void timerOfPic_Tick(object sender, EventArgs e)
        {
            currentIntegrationTime = GetIntegrationTime(CbUnit, tBIntegral);
            currentAverage = GetAverage(tBAverTimes);
            currentBoxcarWidth = GetBoxcarWidth(tBSmooth);

            Thread th = new Thread(InitPixelsAndUpdatePicControlTimer);
            thTemp = th;
            th.Name = "ContinuePic";
            th.IsBackground = true;
            th.Start();
        }

        private void InitPixelsAndUpdatePicControlTimer()
        {
            timerOfPic.Stop();

            InitPixelsAndUpdatePic();

            timerOfPic.Start();
        }

        private void InitPixelsAndUpdatePic()
        {
            pixels = Spectrograph.Spectrograph.GetPixels(currentIntegrationTime, currentAverage, currentBoxcarWidth);
            if (pixels == null || pixels.Count() <= 0)
            {
                return;
            }

            this.Dispatcher.Invoke(updatePicDelegate);
        }

        void UpdatePicMethod()
        {
            UpdatePic(pixels);
        }

        private void AjustWin_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBoxItem item = new ComboBoxItem();
            item.Content = Constants.MILISECOND;
            CbUnit.Items.Add(item);

            item = new ComboBoxItem();
            item.Content = Constants.SECOND;
            CbUnit.Items.Add(item);

            //遍历参数设置，生成测量角度和光阑的组合，并初始化ajustParamList
            InitAjustParamList(ProjectParamWindow.ParametersList);

            if (isContinueUpdatingPic)
            {
                timerOfPic.Start();
            }

            //校准流程
            Thread ajustThread = new Thread(Ajusting);
            ajustThread.IsBackground = true;
            ajustThread.Start();
        }

        private void ShowMsgBox(String msgBoxTitle, String msgBoxTip, MessageBoxButton msgBoxBtn, bool isNoControl)
        {
            MsgBox.msgBoxTitle = msgBoxTitle;
            MsgBox.msgBoxTip = msgBoxTip;
            MsgBox.msgBoxBtn = msgBoxBtn;
            MsgBox.isNoControl = isNoControl;

            this.Dispatcher.BeginInvoke(openMsgBox);
        }

        public void OpenMsgBoxMethod()
        {
            msgBox = new MsgBox(MsgBox.msgBoxTitle, MsgBox.msgBoxTip, MsgBox.msgBoxBtn, MsgBox.isNoControl);
            msgBox.Owner = this;
            msgBox.ShowDialog();
        }

        //关闭当前打开的提示窗口
        public void CloseMsgWin()
        {
            if (null != msgBox && msgBox.Visibility == Visibility.Visible)
            {
                AjustWindow.msgBox.Close();
            }
        }

        //持续采集并刷新界面图
        private void btnContinuousScan_Click(object sender, RoutedEventArgs e)
        {
            StopContinueScan();

            timerOfPic.Start();

            isContinueUpdatingPic = true;
        }

        //校准
        private void Ajusting()
        {
            //光谱仪移动到校准位置，需要提示用户，等移动到了后自动关闭
            ShowMsgBox(Constants.WARNING, Constants.MOVING_TO_AJUST_POS, MessageBoxButton.OK, true);
            Plc.Plc.ControlPlc(Plc.Plc.AjustPosX, Plc.Plc.AjustPosY, Plc.Plc.DefaultAngle);
            this.Dispatcher.Invoke(closeMsgBox);

            int stop = -1;

            //遍历ProjectParam.ParametersList进行校准测量
            foreach (AjustParam ajustParam in ajustParamList)
            {
                Spectrograph.Spectrograph.SetStrobeEnable(true);

                //提示用户更改光阑
                if (ajustParam.Stop != stop)
                {
                    this.Dispatcher.Invoke(imformUserModifyStop, ajustParam.Stop);
                }
                stop = ajustParam.Stop;

                //调整角度,只有调整好角度才能进行后续操作
                ShowMsgBox(Constants.WARNING, Constants.AJUSTING_ANGLE + ajustParam.Angle, MessageBoxButton.OK, true);
                Plc.Plc.ControlPlc(Plc.Plc.CurrentPositionX, Plc.Plc.CurrentPositionY, (UInt32)ajustParam.Angle);
                this.Dispatcher.Invoke(closeMsgBox);

                ShowMsgBox(Constants.TIP, "当前角度是" + ajustParam.Angle +
                    "，请将积分时间，平均次数和平滑度调整至合适的值\r\n" 
                    + "并依次点击\"存储参考光谱\"和\"存储暗光谱\"进行校准", MessageBoxButton.OK, false);

                //设置按钮可见
                updateBtnsThread = new SetButtonThread(this, true, false, false);
                updateButtonsDelegate = new UpdateButtonsDelegate(updateBtnsThread.UpdateBtns);
                this.Dispatcher.Invoke(updateButtonsDelegate);

                //等待用户点击保存参考光谱按钮
                areSaveRef.WaitOne();

                //等待用户点击保存暗光谱按钮
                areSaveDark.WaitOne();

                currentAjustIndex++;
            }

            //设置用只能点击开始测试按钮
            updateBtnsThread = new SetButtonThread(this, false, false, true);
            updateButtonsDelegate = new UpdateButtonsDelegate(updateBtnsThread.UpdateBtns);
            this.Dispatcher.Invoke(updateButtonsDelegate);
        }

        //提示用户去修改光阑
        private void ImformUserToModifyStopMethod(int stop)
        {
            msgBox = new MsgBox("提示", Constants.MODIFY_STOP1 + stop + Constants.MODIFY_STOP2, MessageBoxButton.OK);
            msgBox.Owner = this;
            msgBox.ShowDialog();
        }

        private void btnSaveRef_Click(object sender, RoutedEventArgs e)
        {
            //停止连续扫描
            StopContinueScan();

            btnSaveRef.IsEnabled = false;
            btnSaveDark.IsEnabled = true;

            //使用光谱仪测量一次，并绘图
            pixels = Spectrograph.Spectrograph.GetRefPixels(GetIntegrationTime(CbUnit, tBIntegral), GetAverage(tBAverTimes), GetBoxcarWidth(tBSmooth));
            if (pixels == null || pixels.Count() <= 0)
            {
                return;
            }

            UpdatePic(pixels);

            ajustParamList[currentAjustIndex].IntegrationTime_us = GetIntegrationTime(CbUnit, tBIntegral);
            ajustParamList[currentAjustIndex].Average = GetAverage(tBAverTimes);
            ajustParamList[currentAjustIndex].Smooth = GetBoxcarWidth(tBSmooth);

            for (int i = 0; i < ajustParamList[currentAjustIndex].Refrence.Count(); i++)
            {
                ajustParamList[currentAjustIndex].Refrence[i] = pixels[i];
            }

            SetImagePic(ANDENG_PIC_PATH);

            SetBtnsEnable(false);

            if (isContinueUpdatingPic)
            {
                timerOfPic.Start();
            }

            areSaveRef.Set();
        }

        private void btnSaveDark_Click(object sender, RoutedEventArgs e)
        {
            //停止连续扫描
            StopContinueScan();

            btnSaveRef.IsEnabled = false;
            btnSaveDark.IsEnabled = true;

            //使用光谱仪测量一次，并绘图
            double[] pixels = Spectrograph.Spectrograph.GetDarkPixels(GetIntegrationTime(CbUnit, tBIntegral), GetAverage(tBAverTimes), GetBoxcarWidth(tBSmooth));
            if (pixels == null || pixels.Count() <= 0)
            {
                return;
            }

            UpdatePic(pixels);

            ajustParamList[currentAjustIndex].IntegrationTime_us = GetIntegrationTime(CbUnit, tBIntegral);
            ajustParamList[currentAjustIndex].Average = GetAverage(tBAverTimes);
            ajustParamList[currentAjustIndex].Smooth = GetBoxcarWidth(tBSmooth);

            for (int i = 0; i < ajustParamList[currentAjustIndex].Dark.Count(); i++)
            {
                ajustParamList[currentAjustIndex].Dark[i] = pixels[i];
            }

            SetImagePic(LIANGDENG_PIC_PATH);

            SetBtnsEnable(true);

            if (isContinueUpdatingPic)
            {
                timerOfPic.Start();
            }

            areSaveDark.Set();
        }

        //设置显示图片
        private void SetImagePic(String imagePath)
        {
            BitmapImage bitMapImage = new BitmapImage();
            bitMapImage.BeginInit();
            bitMapImage.UriSource = new Uri(imagePath, UriKind.Relative);
            bitMapImage.EndInit();
            Image.Source = bitMapImage;
        }

        //点击扫描一次按钮
        private void BtnScanOnce_Click(object sender, RoutedEventArgs e)
        {
            isContinueUpdatingPic = false;

            StopContinueScan();

            currentIntegrationTime = GetIntegrationTime(CbUnit, tBIntegral);
            currentAverage = GetAverage(tBAverTimes);
            currentBoxcarWidth = GetBoxcarWidth(tBSmooth);

            Thread th = new Thread(InitPixelsAndUpdatePic);
            th.IsBackground = true;
            th.Start();
        }
        
        private void btnPauseScan_Click(object sender, RoutedEventArgs e)
        {
            StopContinueScan();

            isContinueUpdatingPic = false;
        }

        private void StopContinueScan()
        {
            timerOfPic.Stop();

            if (null != thTemp)
            {
                thTemp.Abort();
            }
        }

        private void UpdatePic(double[] pixels)
        {
            ObservableCollection<KeyValuePair<double, double>> lineList = new ObservableCollection<KeyValuePair<double, double>>();

            for (int i = 0; i < pixels.Count(); i = i + WRITE_PIC_INVERVEL)
            {
                lineList.Add(new KeyValuePair<double, double>(Spectrograph.Spectrograph.WavelengthArray[i], pixels[i]));
            }

            //刷新光谱图
            TestResultChart.Series.Clear();

            LineSeries ls = new LineSeries();
            ls.IndependentValueBinding = new System.Windows.Data.Binding("Key");//X轴
            ls.DependentValueBinding = new System.Windows.Data.Binding("Value");//Y轴
            ls.DataPointStyle = Application.Current.Resources["MyLineDataPointStyle"] as Style;
            ls.ItemsSource = lineList;

            TestResultChart.Series.Add(ls);
        }

        private void SetBtnsEnable(bool isEnable)
        {
            tBIntegral.IsEnabled = isEnable;
            tBAverTimes.IsEnabled = isEnable;
            tBSmooth.IsEnabled = isEnable;

            CbUnit.IsEnabled = isEnable;

            btnIncreaseTime.IsEnabled = isEnable;
            btnDecreaseTime.IsEnabled = isEnable;
            btnIncreaseTimes.IsEnabled = isEnable;
            btnDecreaseTimes.IsEnabled = isEnable;
            btnIncreaseSmooth.IsEnabled = isEnable;
            btnDecreaseSmooth.IsEnabled = isEnable;
        }

        #region 校准参数处理

        private void InitAjustParamList(ObservableCollection<Model.ProjectParam> projectParamList)
        {
            ajustParamList.Clear();

            foreach (ProjectParam projectParamObject in projectParamList)
            {
                if (!IsContained(ajustParamList, projectParamObject))
                {
                    AjustParam ajustParam = new AjustParam(projectParamObject.MeasureAngle, projectParamObject.Stop);
                    ajustParamList.Add(ajustParam);
                }
            }

            //按照stop进行排序
            SortAjustParamListByStop(ref ajustParamList);

            //根据Angle进行排序
            SortAjustParamListByAngleSameStop(ref ajustParamList);
        }

        //按照Stop进行排序
        private void SortAjustParamListByStop(ref List<AjustParam> ajustParamList)
        {
            for (int i = 0; i < ajustParamList.Count; i++)
            {
                for (int j = 0; i + j < ajustParamList.Count - 1; j++)
                {
                    if (ajustParamList[j].Stop > ajustParamList[j + 1].Stop)
                    {
                        AjustParam temp = ajustParamList[j];
                        ajustParamList[j] = ajustParamList[j + 1];
                        ajustParamList[j + 1] = temp;
                    }
                }
            }
        }

        //按照Angle进行排序
        private void SortAjustParamListByAngle(ref List<AjustParam> ajustParamList)
        {
            for (int i = 0; i < ajustParamList.Count; i++)
            {
                for (int j = 0; i + j < ajustParamList.Count - 1; j++)
                {
                    if (ajustParamList[j].Angle > ajustParamList[j + 1].Angle)
                    {
                        AjustParam temp = ajustParamList[j];
                        ajustParamList[j] = ajustParamList[j + 1];
                        ajustParamList[j + 1] = temp;
                    }
                }
            }
        }

        //对于相同的光阑中对Angle进行排序
        private void SortAjustParamListByAngleSameStop(ref List<AjustParam> ajustParamList)
        {
            List<AjustParam> result = new List<AjustParam>();

            //获取每一段相同Stop的校准参数list
            int stop = -1;
            List<AjustParam> sameStopList = new List<AjustParam>();
            foreach (AjustParam ajustParam in ajustParamList)
            {
                if (ajustParam.Stop == stop || -1 == stop)
                {
                    sameStopList.Add(ajustParam);
                    stop = ajustParam.Stop;
                }
                else
                {
                    //对Angle进行排序并加入结果List
                    SortAjustParamListByAngle(ref sameStopList);
                    result.AddRange(sameStopList);

                    stop = ajustParam.Stop;

                    sameStopList.Clear();
                    sameStopList.Add(ajustParam);
                }
            }

            SortAjustParamListByAngle(ref sameStopList);
            result.AddRange(sameStopList);

            ajustParamList = result;
        }

        private bool IsContained(List<AjustParam> ajustParamList, ProjectParam projectParamObject)
        {
            foreach (AjustParam ajustParam in ajustParamList)
            {
                if ((ajustParam.Angle == projectParamObject.MeasureAngle)
                    && (ajustParam.Stop == projectParamObject.Stop))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        private string increaseClick(string val)
        {
            int intVal = Convert.ToInt32(val);
            return (intVal + 1).ToString();
        }

        private string decreaseClick(string val)
        {
            int intVal = Convert.ToInt32(val);
            if (intVal > 1)
            {
                return (intVal - 1).ToString();
            }
            else
            {
                return val;
            }
        }

        private void btnIncreaseTime_Click(object sender, RoutedEventArgs e)
        {
            tBIntegral.Text = increaseClick(tBIntegral.Text);
        }

        private void btnDecreaseTime_Click(object sender, RoutedEventArgs e)
        {
            tBIntegral.Text = decreaseClick(tBIntegral.Text);
        }

        private void btnIncreaseTimes_Click(object sender, RoutedEventArgs e)
        {
            tBAverTimes.Text = increaseClick(tBAverTimes.Text);
        }

        private void btnDecreaseTimes_Click(object sender, RoutedEventArgs e)
        {
            tBAverTimes.Text = decreaseClick(tBAverTimes.Text);
        }

        private void btnIncreaseSmooth_Click(object sender, RoutedEventArgs e)
        {
            tBSmooth.Text = increaseClick(tBSmooth.Text);
        }

        private void btnDecreaseSmooth_Click(object sender, RoutedEventArgs e)
        {
            tBSmooth.Text = decreaseClick(tBSmooth.Text);
        }

        private void btnStartTest_Click(object sender, RoutedEventArgs e)
        {
            StopContinueScan();

            TestResultChart.Series.Clear();

            this.Hide();

            ResultWindow resWindow = new ResultWindow();
            resWindow.Show();
        }

        
    }

    class SetButtonThread
    {
        private bool isSaveRefBtnEnable = false;
        private bool isSaveDarkBtnEnable = false;
        private bool isStartTestBtnEnable = false;
        private AjustWindow ajustWindow;

        public SetButtonThread(AjustWindow ajustWindow, bool isGetRefBtnEnable, bool isGetDarkBtnEnable, bool isStartTestBtnEnable)
        {
            this.ajustWindow = ajustWindow;
            this.isSaveRefBtnEnable = isGetRefBtnEnable;
            this.isSaveDarkBtnEnable = isGetDarkBtnEnable;
            this.isStartTestBtnEnable = isStartTestBtnEnable;

        }

        public void UpdateBtns()
        {
            this.ajustWindow.btnSaveRef.IsEnabled = isSaveRefBtnEnable;
            this.ajustWindow.btnSaveDark.IsEnabled = isSaveDarkBtnEnable;
            this.ajustWindow.btnStartTest.IsEnabled = isStartTestBtnEnable;
        }


    }
}

